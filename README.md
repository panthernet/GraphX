# GraphX for .NET
[![Build status](https://ci.appveyor.com/api/projects/status/6s7bkthpq14lqb5s?svg=true)](https://ci.appveyor.com/project/panthernet/graphx)[![GraphX](https://buildstats.info/nuget/GraphX)](https://www.nuget.org/packages/GraphX/)

### Introduction 

GraphX for .NET is an advanced open-source graph layout and visualization library that supports different layout algorithms and provides many means for visual customizations It is capable of rendering large amount of vertices and steadily moves to support the most popular .NET platforms. GraphX already served well as the foundation for many other projects where its functionality was irreplaceable.

### Requirements
GraphX requires **Visual Studio 2019 Community Edition** or above to build manually.
[**QuickGraphCore**](https://www.nuget.org/packages/QuickGraphCore/) nuget project is required for GraphX to operate. Also it is worth noting that it uses partial code from: Graph#, WPFExtensions, NodeXL, Extended WPF Toolkit, YAXLib and ModernUI.

### Platform Support
Our library supports following platforms:
* Windows Desktop (.NET Core/WPF/WinForms on Windows 7+ using .NET4.6.1+ or .NET Core 3.1)
* Universal Windows Platform (UWP or UAP) on Windows 10
* Xamarin/Uno (WIP, only logic core is available)

### Features

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
  * Support for parameterized edge routing algorithms (SimpleER, EdgeBundling, PathFinder)
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
