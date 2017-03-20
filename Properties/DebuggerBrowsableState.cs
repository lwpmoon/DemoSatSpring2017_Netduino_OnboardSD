using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DemoSat_Multithreaded")]
[assembly: AssemblyDescription("Flight code for ACC DemoSat payloads")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Arapahoe Community College")]
[assembly: AssemblyProduct("DemoSat_Multithreaded")]
[assembly: AssemblyCopyright("Copyright Colorado Space Grant Consortium © 2017")]
[assembly: AssemblyTrademark("DemoSat TM")]
[assembly: AssemblyCulture("")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// ReSharper disable once CheckNamespace
namespace System.Diagnostics
{
    //  DebuggerBrowsableState states are defined as follows:
    //      Never       never show this element
    //      Expanded    expansion of the class is done, so that all visible internal members are shown
    //      Collapsed   expansion of the class is not performed. Internal visible members are hidden
    //      RootHidden  The target element itself should not be shown, but should instead be
    //                  automatically expanded to have its members displayed.
    //  Default value is collapsed

    //  Please also change the code which validates DebuggerBrowsableState variable (in this file)
    //  if you change this enum.
    public enum DebuggerBrowsableState
    {
        Never = 0,
        Collapsed = 2,
        RootHidden = 3
    }
}