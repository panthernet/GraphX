GraphX for .NET v2 PCL-COMPLIANT VERSION
http://www.panthernet.ru

Project Description
  GraphX for .NET is an advanced graph visualization library based on Graph# algorithmic blueprints that uses WPF for rendering (WinForms interop is also working). I've coded it when i felt that Graph# is outdated and can't be easily enhanced in the means of performance and feature implementation.
  
  Library depends on:
  
    * QuickGraph
    * YAXLib
  
  Library uses partial code from:
  
    * Graph#
    * WPFExtensions
    * NodeXL
    * Extended WPF Toolkit
    
  Features:

    Performance oriented, isolated library design optimized for:
		Large amount of graph vertices rendering
		Separated visual and logic libraries usage		
		Constantly improving MVVM support		
    Internal support for Graph# algorithms ( FR, KK, ISOM, LinLog, Simple Tree, Simple Circle, Sugiyama, CompoundFDP, FSA/FSAOneWay overlap removal)
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
		Easy vertex drag and highlight support
		Customizable vertex labels support 
		Support for different vertex math shapes for proper edge connections rendering
    Easy support for user-defined external layout, overlap removal and edge routing algorithms
    Configurable vertex and edge controls highlighting system based on behaviour mechanics
    Vertex and edge move, delete, add, mouse over animation support with the ability to create custom animations!
    Universal graph Save/Load methods using YAXLib interfaces
    Graph state saving and loading allows the capture and store in-memory visual and data graphs
    Async algorithm computation support
    Rich usability documentation

  Additional Features:

    Built in enchanced zoom control with minimap and zooming features:
        Support for area selection of the vertices
        Support for area zooming and smooth animations
    Design-time visual preview for all controls
    Several example projects
