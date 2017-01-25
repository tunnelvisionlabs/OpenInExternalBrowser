// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Tvl.VisualStudio.OpenInExternalBrowser")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tunnel Vision Laboratories, LLC")]
[assembly: AssemblyProduct("Tvl.VisualStudio.OpenInExternalBrowser")]
[assembly: AssemblyCopyright("Copyright © Sam Harwell 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c8478eb2-9f48-49fe-8ef2-61d198e01bfc")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.1.0")]
[assembly: AssemblyInformationalVersion("1.1.1.0-dev")]

[assembly: ProvideCodeBase(
    AssemblyName = "Tvl.VisualStudio.Shell.Utility.10",
    Version = "1.0.0.0",
    CodeBase = "$PackageFolder$\\Tvl.VisualStudio.Shell.Utility.10.dll")]
[assembly: ProvideCodeBase(
    AssemblyName = "Tvl.VisualStudio.Text.Utility.10",
    Version = "1.0.0.0",
    CodeBase = "$PackageFolder$\\Tvl.VisualStudio.Text.Utility.10.dll")]
