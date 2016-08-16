**GraphX for .NET (Core Branch)**
http://panthernet.org 

For **GraphX for .NET PRO version** please visit http://graphx.pro


**What Is This?**

GraphX for .NET is an advanced open-source graph layout and visualization library that supports different layout algorithms and provides many means for visual customizations It is capable of rendering large amount of vertices and steadily moves to support the most popular .NET platforms. GraphX already served well as the foundation for many other projects where its functionality was irreplaceble.



**Any Requirements?**

Due to C#6.0 compliance GraphX requires **Visual Studio 2015 Comunity Edition** or above.
**QuickGraphPCL** nuget project is required for GraphX to operate. Also it is worth noting that it uses partial code from:
* Graph#
* WPFExtensions
* NodeXL
* Extended WPF Toolkit
  
And library showcase uses:
* YAXLib
* ModernUI



**How Flexible It Is?**

We're aim to support as much platforms as possible and we already made our logic core with algorithms to support wide range of platforms.
Our library supports following platforms:
* Windows Desktop (WPF & WinForms on Windows XP SP3 using .NET4.0)
* Universal Windows Application (UWA) using .NET 4.5
* Windows Metro 8.1 using .NET 4.5
* Microsoft Silverlight 5 (WIP, only logic core is available)
* Windows Phone 8/10 (WIP, only logic core is available)
* Xamarin (WIP, only logic core is available)



**What Exactly I Can Do With It?**

* GraphX is a performance oriented library coded with modular design in mind optimized for:
  * Large amount of templated graph vertices rendering
  * Isolated visual and logic libraries design and modular coding approach for better extensibility
  * Constantly improving MVVM support
  * .NET multiplatform support
  * Multiple layout algorithms ( FR, KK, ISOM, LinLog, Simple Tree, Simple Circle, Sugiyama, CompoundFDP, FSA/FSAOneWay overlap removal) and for grouped graph layout algorithm

* In general it can do almost everything you need to layout and display any graph you want, in particular it provide following features:
  * Ability to create and plug-in custom external layout, overlap removal and edge routing algorithms
  * Enhanced edge pointer customization capabilities allowing to easily create and apply custom edge pointers
  * Customizable control highlighting using behaviour logic
  * Graph printing methods for Windows Desktop platform
  * Vertex and edge move, delete, add, mouse over animation support with the ability to create custom animations!
  * Universal graph serialization methods implemented by shared interface allows custom serialization to be applied on different platforms
  * Graph state saving and loading allows the capture and store in-memory visual and data graphs
  * Async algorithm computation support
  * Rich usability documentation and sample projects

* It supports following edge related features:
  * Support for parametrized edge routing algorithms (SimpleER, EdgeBundling, PathFinder)
  * Support for dynamic and/or single edge routing calculation (for ex. for dragged vertex)
  * Edges curving (smoothing) technique that can be applied to any ER algorithm
  * Dynamic templated edge labels with edge alignment support
  * Easy templating including dashed edges of several types
  * Optional self-looped edges visualization support
  * Optional parallel edges visualization support between vertices

* Advanced graph vertex features are as follows:
  * Easy vertex drag and highlight support including on-the-fly edge routing updates
  * Filtering feature provides selective vertex rendering leaving supplied graph untouched
  * Customizable vertex labels support that allows to set text, position and angle
  * Support for different vertex math shapes for proper edge connections rendering
  * Support for different vertex and edge animations including the ability to easily create custom animations
  * Vertex connection points (VCP) allows to implement customizable edge-to-vertex connections
  * Vertex snap-to-grid feature while dragging vertex or group of vertices

* And at last some additional features to note:
  * Built in enchanced zoom control with minimap and zooming features:
    * Support for area selection of the vertices
    * Support for area zooming and smooth animations
  * Design-time visual preview for all controls
  * Many well commented example projects
