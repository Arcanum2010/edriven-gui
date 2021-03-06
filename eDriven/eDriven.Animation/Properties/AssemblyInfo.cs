﻿using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

#if !TRIAL
[assembly: AssemblyTitle("eDriven.Animation")]
#endif

#if TRIAL
[assembly: AssemblyTitle("eDriven.Animation Free Edition")]
#endif

[assembly: AssemblyDescription("eDriven animation assembly")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]

#if !TRIAL
[assembly: AssemblyProduct("eDriven.Animation")]
#endif

#if TRIAL
[assembly: AssemblyProduct("eDriven.Animation Free Edition")]
#endif

[assembly: AssemblyCopyright("Copyright © Danko Kozar 2010-2014. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a706ba71-5b84-4faf-8509-72a02ce6aff0")]

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
[assembly: AssemblyVersion("2.4.0.0")]
[assembly: AssemblyFileVersion("2.4.0.0")]
