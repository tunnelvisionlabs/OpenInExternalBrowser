// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved. Licensed under the Microsoft Reciprocal License (MS-RL). See LICENSE in the project root for license information.

namespace Tvl.VisualStudio.OpenInExternalBrowser
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.TextManager.Interop;
    using FileNotFoundException = System.IO.FileNotFoundException;
    using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
    using Process = System.Diagnostics.Process;

    internal class UrlCommandFilter : TextViewCommandFilter
    {
        private readonly ITextView _textView;

        private readonly SVsServiceProvider _serviceProvider;

        private readonly ITagAggregator<IUrlTag> _urlTagAggregator;

        public UrlCommandFilter(IVsTextView textViewAdapter, ITextView textView, UrlCommandFilterProvider provider)
            : base(textViewAdapter)
        {
            if (textView == null)
                throw new ArgumentNullException("textView");
            if (provider == null)
                throw new ArgumentNullException("provider");

            _textView = textView;
            _serviceProvider = provider.ServiceProvider;
            _urlTagAggregator = provider.ViewTagAggregatorFactoryService.CreateTagAggregator<IUrlTag>(textView);
        }

        protected ITextView TextView
        {
            get
            {
                return _textView;
            }
        }

        protected override bool HandlePreExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (commandGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                switch ((VSConstants.VSStd2KCmdID)commandId)
                {
                case VSConstants.VSStd2KCmdID.OPENURL:
                    if (pvaIn != IntPtr.Zero)
                    {
                        int line = (int)Marshal.GetObjectForNativeVariant(pvaIn);
                        int column = (int)Marshal.GetObjectForNativeVariant(new IntPtr(pvaIn.ToInt32() + 16));
                        return TryOpenUrlAtPoint(line, column);
                    }

                    return TryOpenUrlAtCaret();

                default:
                    break;
                }
            }

            return base.HandlePreExec(ref commandGroup, commandId, executionOptions, pvaIn, pvaOut);
        }

        private bool TryOpenUrlAtCaret()
        {
            SnapshotPoint bufferPosition = TextView.Caret.Position.BufferPosition;
            ITextSnapshotLine containingLine = bufferPosition.GetContainingLine();
            int line = containingLine.LineNumber;
            int column = bufferPosition - containingLine.Start;
            return TryOpenUrlAtPoint(line, column);
        }

        private bool TryOpenUrlAtPoint(int line, int column)
        {
            ITagSpan<IUrlTag> tagSpan;
            if (!TryGetUrlSpan(line, column, out tagSpan))
                return false;

            if (!IsUrlSpanValid(tagSpan))
                return false;

            return OpenUri(tagSpan.Tag.Url);
        }

        private bool TryGetUrlSpan(int line, int column, out ITagSpan<IUrlTag> urlSpan)
        {
            urlSpan = null;

            SnapshotPoint point;
            if (!TryToSnapshotPoint(TextView.TextSnapshot, line, column, out point))
                return false;

            SnapshotSpan span = new SnapshotSpan(point, 0);
            foreach (IMappingTagSpan<IUrlTag> current in _urlTagAggregator.GetTags(span))
            {
                NormalizedSnapshotSpanCollection spans = current.Span.GetSpans(TextView.TextSnapshot);
                if (spans.Count == 1 && spans[0].Length == current.Span.GetSpans(current.Span.AnchorBuffer)[0].Length && spans[0].Contains(span.Start))
                {
                    urlSpan = new TagSpan<IUrlTag>(spans[0], current.Tag);
                    return true;
                }
            }

            return false;
        }

        private static bool TryToSnapshotPoint(ITextSnapshot snapshot, int startLine, int startIndex, out SnapshotPoint result)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");

            result = default(SnapshotPoint);
            if (snapshot == null || startLine < 0 || startLine >= snapshot.LineCount || startIndex < 0)
                return false;

            ITextSnapshotLine startSnapshotLine = snapshot.GetLineFromLineNumber(startLine);
            if (startIndex > startSnapshotLine.Length)
                return false;

            result = startSnapshotLine.Start + startIndex;
            return true;
        }

        private static bool IsUrlSpanValid(ITagSpan<IUrlTag> urlTagSpan)
        {
            return urlTagSpan != null
                && urlTagSpan.Tag != null
                && urlTagSpan.Tag.Url != null
                && urlTagSpan.Tag.Url.IsAbsoluteUri;
        }

        private bool OpenUri(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            if (!uri.IsAbsoluteUri)
                return false;

            /* First try to use the Web Browsing Service. This is not known to work because the
             * CreateExternalWebBrowser method always returns E_NOTIMPL. However, it is presumably
             * safer than a Shell Execute for arbitrary URIs.
             */
            IVsWebBrowsingService service = _serviceProvider.GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;
            if (service != null)
            {
                __VSCREATEWEBBROWSER createFlags = __VSCREATEWEBBROWSER.VSCWB_AutoShow;
                VSPREVIEWRESOLUTION resolution = VSPREVIEWRESOLUTION.PR_Default;
                int result = ErrorHandler.CallWithCOMConvention(() => service.CreateExternalWebBrowser((uint)createFlags, resolution, uri.AbsoluteUri));
                if (ErrorHandler.Succeeded(result))
                    return true;
            }

            // Fall back to Shell Execute, but only for http or https URIs
            if (uri.Scheme != "http" && uri.Scheme != "https")
                return false;

            try
            {
                Process.Start(uri.AbsoluteUri);
                return true;
            }
            catch (Win32Exception)
            {
            }
            catch (FileNotFoundException)
            {
            }

            return false;
        }
    }
}
