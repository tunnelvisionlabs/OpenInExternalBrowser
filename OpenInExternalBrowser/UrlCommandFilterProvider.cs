// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tvl.VisualStudio.OpenInExternalBrowser
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.Utilities;
    using Tvl.VisualStudio.Text;

    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal class UrlCommandFilterProvider : IVsTextViewCreationListener
    {
        [ImportingConstructor]
        public UrlCommandFilterProvider(SVsServiceProvider serviceProvider, IVsEditorAdaptersFactoryService editorAdaptersFactoryService, IViewTagAggregatorFactoryService viewTagAggregatorFactoryService)
        {
            this.ServiceProvider = serviceProvider;
            this.EditorAdaptersFactoryService = editorAdaptersFactoryService;
            this.ViewTagAggregatorFactoryService = viewTagAggregatorFactoryService;
        }

        public SVsServiceProvider ServiceProvider
        {
            get;
            private set;
        }

        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService
        {
            get;
            private set;
        }

        public IViewTagAggregatorFactoryService ViewTagAggregatorFactoryService
        {
            get;
            private set;
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            UrlCommandFilter commandFilter = new UrlCommandFilter(textViewAdapter, textView, this);
            commandFilter.Enabled = true;
            textView.Properties.AddProperty(typeof(UrlCommandFilter), commandFilter);
        }
    }
}
