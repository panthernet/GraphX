GraphX for .NET v2 PCL-COMPLIANT VERSION (Core Branch)
http://www.panthernet.ru

Project Description

GraphX for .NET is an advanced open-source graph visualization library that supports different layout algorithms and highly customizable features. It is capable of rendering large amount of vertices and steadily moves to support the most popular .NET platforms. GraphX already served well as the foundation for many other projects where its functionality was irreplaceble.

Main GraphX libraries can be used in both C# and VB for .NET using WPF, WinForms or METRO technologies.

  Library depends on:
  
    * QuickGraph
  
  Library uses partial code from:
  
    * Graph#
    * WPFExtensions
    * NodeXL
    * Extended WPF Toolkit
  
  Library showcase uses:

    * YAXLib
	* ModernUI
	
  Library supports following platforms:
  
    * Windows Desktop (WPF & WinForms on Windows XP SP3 and later)
    * Microsoft Silverlight 5 (WIP, only logic core is available)
    * Windows Metro 8.1 and later (BETA)
    * Windows Phone 8 (WIP, only logic core is available)
    
  Features:

    Performance oriented, isolated library design optimized for:
        Large amount of graph vertices rendering
        Isolated visual and logic libraries design and modular coding approach
        Constantly improving MVVM support
        .NET multiplatform support	
    Default support for layout algorithms ( FR, KK, ISOM, LinLog, Simple Tree, Simple Circle, Sugiyama, CompoundFDP, FSA/FSAOneWay overlap removal)
    Advanced graph edges features:
        Support for parametrized edge routing algorithms (SimpleER, EdgeBundling, PathFinder)
        Support for dynamic and/or single edge routing calculation (for ex. for dragged vertex)
        Edges curving (smoothing) technique that can be applied to any ER algorithm
        Dynamic templated edge labels with edge alignment support
        Easy templating including dashed edges of several types
        Edge arrow drawing control
        Optional self-looped edges visualization support
        Optional parallel edges visualization support between vertices
	Advanced graph vertex features:
		Easy vertex drag and highlight support including on-the-fly edge routing updates
		Filtering feature provides selective vertex rendering leaving supplied graph untouched
		Customizable vertex labels support that allows to set text, positionand angle
		Support for different vertex math shapes for proper edge connections rendering
		Support for different vertex and edge animations including the ability to easily create custom animations
    Easy support for user-defined external layout, overlap removal and edge routing algorithms
    Configurable vertex and edge controls highlighting system based on behaviour mechanics
    Vertex and edge move, delete, add, mouse over animation support with the ability to create custom animations!
    Universal graph serialization methods using YAXLib interfaces
    Graph state saving and loading allows the capture and store in-memory visual and data graphs
    Async algorithm computation support
    Rich usability documentation

  Additional Features:

    Built in enchanced zoom control with minimap and zooming features:
        Support for area selection of the vertices
        Support for area zooming and smooth animations
    Design-time visual preview for all controls
    Several example projects
