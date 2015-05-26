using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

[assembly: AssemblyTitle("GraphX Controls Library")]

[assembly: ComVisible(false)]
[assembly: Guid("d8511b14-512b-46ec-ad4f-8f14f46a466c")]

[assembly: XmlnsPrefix("http://schemas.panthernet.ru/graphx/", "graphx")]
[assembly: XmlnsDefinition("http://schemas.panthernet.ru/graphx/",
    "GraphX.Controls")]
[assembly: XmlnsDefinition("http://schemas.panthernet.ru/graphx/",
    "GraphX.Controls.Animations")]
[assembly: XmlnsDefinition("http://schemas.panthernet.ru/graphx/",
    "GraphX.Controls.Models")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly
    //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]