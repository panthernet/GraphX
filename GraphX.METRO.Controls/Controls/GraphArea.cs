using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using GraphX.Controls;
using GraphX.Controls.Models;
using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GraphX.GraphSharp.Algorithms.Layout;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using GraphX.DesignerExampleData;
using GraphX.Models;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace GraphX
{
    public class GraphArea<TVertex, TEdge, TGraph>  : GraphAreaBase, IDisposable
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
    { 

        #region My properties

        public static readonly DependencyProperty LogicCoreProperty =
            DependencyProperty.Register("LogicCore", typeof(IGXLogicCore<TVertex, TEdge, TGraph>), typeof(GraphArea<TVertex, TEdge, TGraph>), new PropertyMetadata(null, logic_core_changed));

        private static void logic_core_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //automaticaly assign default file service provider 
            if(e.NewValue != null && ((IGXLogicCore<TVertex, TEdge, TGraph>)e.NewValue).FileServiceProvider == null)
                ((IGXLogicCore<TVertex, TEdge, TGraph>)e.NewValue).FileServiceProvider = new FileServiceProviderMETRO();
        }

        /// <summary>
        /// Gets or sets GraphX logic core object that will drive this visual
        /// </summary>
        public IGXLogicCore<TVertex, TEdge, TGraph> LogicCore
        {
            get { return (IGXLogicCore<TVertex, TEdge, TGraph>)GetValue(LogicCoreProperty); }
            set { SetValue(LogicCoreProperty, value); }
        }


        public IGraphControlFactory ControlFactory { get; set; }

        /// <summary>
        /// Gets logic core unsafely converted to specified type
        /// </summary>
        /// <typeparam name="T">Logic core type</typeparam>
        public T GetLogicCore<T>()
        {
            return (T)LogicCore;
        }

        public void SetLogicCore(IGXLogicCore<TVertex, TEdge, TGraph> core)
        {
            LogicCore = core;
        }

        /// <summary>
        /// Gets or sets if visual properties such as edge dash style or vertex shape should be automaticaly reapplied to visuals when graph is regenerated.
        /// True by default.
        /// </summary>
        public bool EnableVisualPropsRecovery { get; set; }
        /// <summary>
        /// Gets or sets if visual properties such as edge dash style or vertex shape should be automaticaly applied to newly added visuals which are added using AddVertex() or AddEdge() or similar methods.
        /// True by default.
        /// </summary>
        public bool EnableVisualPropsApply { get; set; }

        /// <summary>
        /// Link to LogicCore. Gets if edge routing is used.
        /// </summary>
        internal override bool IsEdgeRoutingEnabled { get { return LogicCore != null && LogicCore.IsEdgeRoutingEnabled; } }
        /// <summary>
        /// Link to LogicCore. Gets self looped edge radius.
        /// </summary>
        internal override double EdgeSelfLoopCircleRadius { get { return LogicCore == null ? 0 : LogicCore.EdgeSelfLoopCircleRadius; } }
        /// <summary>
        /// Link to LogicCore. Gets if self looped edges are enabled.
        /// </summary>
        internal override bool EdgeShowSelfLooped { get { return LogicCore != null && LogicCore.EdgeShowSelfLooped; } }
        /// <summary>
        /// Link to LogicCore. Gets if parallel edges are enabled.
        /// </summary>
        internal override bool EnableParallelEdges { get { return LogicCore != null && LogicCore.EnableParallelEdges; } }
        /// <summary>
        /// Link to LogicCore. Gets looped edge offset.
        /// </summary>
        internal override Point EdgeSelfLoopCircleOffset { get { return LogicCore == null ? new Point() : LogicCore.EdgeSelfLoopCircleOffset.ToWindows(); } }
        /// <summary>
        /// Link to LogicCore. Gets if edge curving is used.
        /// </summary>
        internal override bool EdgeCurvingEnabled { get { return LogicCore != null && LogicCore.EdgeCurvingEnabled; } }
        /// <summary>
        /// Link to LogicCore. Gets if edge curving tolerance.
        /// </summary>
        internal override double EdgeCurvingTolerance { get { return LogicCore == null ? 0 : LogicCore.EdgeCurvingTolerance; } }

        /// <summary>
        /// Dummy property. Use EdgesList and VertexList instead.
        /// Also use corresponding methods to modify item collections.
        /// </summary>
       // private new UIElementCollection Children { get { return null; } }

        /// <summary>
        /// Add custom control for 
        /// </summary>
        /// <param name="control"></param>
        public void AddCustomChildControl(UIElement control)
        {
            base.Children.Add(control);
        }

        /// <summary>
        /// Inserts custom control into GraphArea
        /// </summary>
        /// <param name="index">Insertion index</param>
        /// <param name="control">Custom control</param>
        public void InsertCustomChildControl(int index, UIElement control)
        {
            base.Children.Insert(index, control);
        }

        /// <summary>
        /// Remove custom control from GraphArea children.
        /// </summary>
        /// <param name="control">Custom control</param>
        public void RemoveCustomChildControl(UIElement control)
        {
            base.Children.Remove(control);
        }

        #region StateStorage
        /// <summary>
        /// Provides methods for saving and loading graph layout states
        /// </summary>
        public StateStorage<TVertex, TEdge, TGraph> StateStorage { get; private set; }
        #endregion

        readonly Dictionary<TEdge, EdgeControl> _edgeslist = new Dictionary<TEdge, EdgeControl>();
        readonly Dictionary<TVertex, VertexControl> _vertexlist = new Dictionary<TVertex, VertexControl>();

        /// <summary>
        /// Gets edge controls read only collection. To modify collection use AddEdge() RemoveEdge() methods.
        /// </summary>
        public IDictionary<TEdge, EdgeControl> EdgesList
        {
            get {  return _edgeslist; }
        }
        /// <summary>
        /// Gets vertex controls read only collection. To modify collection use AddVertex() RemoveVertex() methods.
        /// </summary>
        public IDictionary<TVertex, VertexControl> VertexList
        {
            get { return _vertexlist; }
        }

        #endregion

        public GraphArea()
        {
            ControlFactory = new GraphControlFactory { FactoryRootArea = this };
            StateStorage = new StateStorage<TVertex, TEdge, TGraph>(this);            
            EnableVisualPropsRecovery = true;
            EnableVisualPropsApply = true;
            //CacheMode = new BitmapCache(2) { EnableClearType = false, SnapsToDevicePixels = true };
            Transitions = new TransitionCollection();
            Transitions.Add(new ContentThemeTransition());
            #region Designer Data
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Width = DesignSize.Width;
                Height = DesignSize.Height;
                var vc = new VertexDataExample(1, "Johnson B.C");
                var ctrl = ControlFactory.CreateVertexControl(vc);
                SetX(ctrl, 0); SetY(ctrl, 0, true);

                var vc2 = new VertexDataExample(2, "Manson J.C");
                var ctrl2 = ControlFactory.CreateVertexControl(vc2);
                SetX(ctrl2, 200); SetY(ctrl2, 0, true);

                var vc3 = new VertexDataExample(1, "Franklin A.J");
                var ctrl3 = ControlFactory.CreateVertexControl(vc3);
                SetX(ctrl3, 100); SetY(ctrl3, 100, true);

                UpdateLayout();
                var edge = new EdgeDataExample<VertexDataExample>(vc, vc2, 1) { Text = "One" };
                var edgectrl = ControlFactory.CreateEdgeControl(ctrl, ctrl2, edge);

                base.Children.Add(edgectrl);

                edge = new EdgeDataExample<VertexDataExample>(vc2, vc3, 1) { Text = "Two" };
                edgectrl = ControlFactory.CreateEdgeControl(ctrl2, ctrl3, edge);
                base.Children.Add(edgectrl);

                edge = new EdgeDataExample<VertexDataExample>(vc3, vc, 1) { Text = "Three" };
                edgectrl = ControlFactory.CreateEdgeControl(ctrl3, ctrl, edge);
                base.Children.Add(edgectrl);


                base.Children.Add(ctrl);
                base.Children.Add(ctrl2);
                base.Children.Add(ctrl3);
            }
            #endregion
        }

        #region Edge & vertex controls operations

        /// <summary>
        /// Returns all existing VertexControls added into the layout as new Array
        /// </summary>
        public override VertexControl[] GetAllVertexControls() { return _vertexlist.Values.ToArray(); }

        #region Remove controls

        /// <summary>
        /// Remove all visual vertices
        /// </summary>
        public void RemoveAllVertices()
        {
            foreach (var item in _vertexlist)
            {
                if (DeleteAnimation != null)
                    DeleteAnimation.AnimateVertex(item.Value);
                else
                {
                    item.Value.Clean();
                    base.Children.Remove(item.Value);
                }
            }
            _vertexlist.Clear();
        }

        /// <summary>
        /// Remove all visual edges
        /// </summary>
        public void RemoveAllEdges()
        {
            foreach (var item in _edgeslist)
            {
                if (DeleteAnimation != null)
                    DeleteAnimation.AnimateEdge(item.Value);
                else
                {
                    item.Value.Clean();
                    base.Children.Remove(item.Value);
                }
            }
            _edgeslist.Clear();
        }

        /// <summary>
        /// Remove vertex from layout
        /// </summary>
        /// <param name="vertexData">Vertex data object</param>
        public void RemoveVertex(TVertex vertexData)
        {
            if (vertexData == null || !_vertexlist.ContainsKey(vertexData)) return;

            var ctrl = _vertexlist[vertexData];
            _vertexlist.Remove(vertexData);

            if (DeleteAnimation != null)
                DeleteAnimation.AnimateVertex(ctrl);
            else
            {
                base.Children.Remove(ctrl);
                ctrl.Clean();
            }
        }

        /// <summary>
        /// Remove edge from layout
        /// </summary>
        /// <param name="edgeData">Edge data object</param>
        public void RemoveEdge(TEdge edgeData)
        {
            if (edgeData == null || !_edgeslist.ContainsKey(edgeData)) return;

            var ctrl = _edgeslist[edgeData];
            _edgeslist.Remove(edgeData);
            
            if (DeleteAnimation != null)
                DeleteAnimation.AnimateEdge(ctrl);
            else
            {
                base.Children.Remove(ctrl);
                ctrl.Clean();
            }
        }
        #endregion

        #region Add controls
        /// <summary>
        /// Add vertex to layout
        /// </summary>
        /// <param name="vertexData">Vertex data object</param>
        /// <param name="vertexControl">Vertex visual control object</param>
        public void AddVertex(TVertex vertexData, VertexControl vertexControl)
        {
            if (AutoAssignMissingDataId && vertexData.ID == -1)
                vertexData.ID = GetNextUniqueId();
            InternalAddVertex(vertexData, vertexControl);
            if (EnableVisualPropsApply && vertexControl != null)
                ReapplySingleVertexVisualProperties(vertexControl);
        }

        private void InternalAddVertex(TVertex vertexData, VertexControl vertexControl)
        {
            if (vertexControl == null || vertexData == null) return;
            vertexControl.RootArea = this;
            if (_vertexlist.ContainsKey(vertexData)) throw new GX_InvalidDataException("AddVertex() -> Vertex with the same data has already been added to layout!");
            _vertexlist.Add(vertexData, vertexControl);
            base.Children.Add(vertexControl);
        }

        /// <summary>
        /// Add an edge to layout. Edge is added into the end of the visual tree causing it to be rendered above all vertices.
        /// </summary>
        /// <param name="edgeData">Edge data object</param>
        /// <param name="edgeControl">Edge visual control</param>
        public void AddEdge(TEdge edgeData, EdgeControl edgeControl)
        {
            if (AutoAssignMissingDataId && edgeData.ID == -1)
                edgeData.ID = GetNextUniqueId();
            InternalAddEdge(edgeData, edgeControl);
            if (EnableVisualPropsApply && edgeControl != null)
                ReapplySingleEdgeVisualProperties(edgeControl);
        }

        private void InternalAddEdge(TEdge edgeData, EdgeControl edgeControl)
        {
            if (edgeControl == null || edgeData == null) return;
            if (_edgeslist.ContainsKey(edgeData)) throw new GX_InvalidDataException("AddEdge() -> An edge with the same data has already been added to layout!");
            edgeControl.RootArea = this;
            _edgeslist.Add(edgeData, edgeControl);
            base.Children.Add(edgeControl);
        }

        /// <summary>
        /// Insert an edge to layout at specified position. By default, edge is inserted into the begining of the visual tree causing it to be rendered below all of the vertices.
        /// </summary>
        /// <param name="edgeData">Edge data object</param>
        /// <param name="edgeControl">Edge visual control</param>
        /// <param name="num">Insert position</param>
        public void InsertEdge(TEdge edgeData, EdgeControl edgeControl, int num = 0)
        {
            if (AutoAssignMissingDataId && edgeData.ID == -1)
                edgeData.ID = GetNextUniqueId();
            InternalInsertEdge(edgeData, edgeControl, num);
            if (EnableVisualPropsApply && edgeControl != null)
                ReapplySingleEdgeVisualProperties(edgeControl);
        }

        private void InternalInsertEdge(TEdge edgeData, EdgeControl edgeControl, int num = 0)
        {
            if (edgeControl == null || edgeData == null) return;
            if (_edgeslist.ContainsKey(edgeData)) throw new GX_InvalidDataException("AddEdge() -> An edge with the same data has already been added!");
            edgeControl.RootArea = this;
            _edgeslist.Add(edgeData, edgeControl);
            try
            {
                base.Children.Insert(num, edgeControl);
            }
            catch (Exception ex)
            {
                throw new GX_GeneralException(ex.Message + ". Probably you have an error in edge template.");
            }
        }

        #endregion

        #endregion

        #region Automatic data ID storage and resolving
        private int _dataIdCounter;
        private int GetNextUniqueId()
        {
            while (_dataIdsCollection.Contains(_dataIdCounter))
            {
                _dataIdCounter++;
            }
            _dataIdsCollection.Add(_dataIdCounter);
            return _dataIdCounter;
        }

        private readonly HashSet<int> _dataIdsCollection = new HashSet<int>();

        #endregion

        #region GenerateGraph

        #region Sizes operations
        /// <summary>
        /// Get vertex control sizes
        /// </summary>
        public Dictionary<TVertex, Measure.Size> GetVertexSizes()
        {          
            //measure if needed and get all vertex sizes
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var vertexSizes = new Dictionary<TVertex, Measure.Size>(_vertexlist.Count);
            //go through the vertex presenters and get the actual layoutpositions
            foreach (var vc in VertexList)
            {
                vertexSizes[vc.Key] = new Measure.Size(vc.Value.ActualWidth, vc.Value.ActualHeight);
            }
            return vertexSizes;
        }

        /// <summary>
        /// Get visual vertex size rectangles (can be used by some algorithms)
        /// </summary>
        /// <param name="positions">Vertex positions collection (auto filled if null)</param>
        /// <param name="vertexSizes">Vertex sizes collection (auto filled if null)</param>
        /// <param name="getCenterPoints">True if you want center points returned instead of top-left (needed by overlap removal algo)</param>
        public Dictionary<TVertex, Measure.Rect> GetVertexSizeRectangles(IDictionary<TVertex, Measure.Point> positions = null, Dictionary<TVertex, Measure.Size> vertexSizes = null, bool getCenterPoints = false)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");

            if (vertexSizes == null) vertexSizes = GetVertexSizes();
            if (positions == null) positions = GetVertexPositions();
            var rectangles = new Dictionary<TVertex, Measure.Rect>();
            foreach (var vertex in LogicCore.Graph.Vertices)
            {
                Measure.Point position; Measure.Size size;
                if (!positions.TryGetValue(vertex, out position) || !vertexSizes.TryGetValue(vertex, out size)) continue;
                if (!getCenterPoints) rectangles[vertex] = new Measure.Rect(position.X, position.Y, size.Width, size.Height);
                else rectangles[vertex] = new Measure.Rect(position.X - size.Width * (float)0.5, position.Y - size.Height * (float)0.5, size.Width, size.Height);
            
            }
            return rectangles;
        }

        /// <summary>
        /// Returns all vertices positions list
        /// </summary>
        public Dictionary<TVertex, Measure.Point> GetVertexPositions()
        {
            return VertexList.ToDictionary(vertex => vertex.Key, vertex => vertex.Value.GetPositionGraphX());
        }

        #endregion

        #region PreloadVertexes()

        /// <summary>
        /// Preloads vertex controls from specified graph. All vertices are created hidden by default.
        /// This method can be used for custom external algorithm implementations or manual visual graph population.
        /// </summary>
        /// <param name="graph">Data graph</param>
        /// <param name="dataContextToDataItem">Sets DataContext property to vertex data item of the control</param>
        /// <param name="forceVisPropRecovery"></param>
        public void PreloadVertexes(TGraph graph, bool dataContextToDataItem = true, bool forceVisPropRecovery = false)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            //clear edge and vertex controls
            RemoveAllVertices();
            RemoveAllEdges();

            //preload vertex controls
            foreach (var it in graph.Vertices)
            {
                var vc = ControlFactory.CreateVertexControl(it);
                vc.DataContext = dataContextToDataItem ? it : null;
                vc.Visibility = Visibility.Visible; // make them invisible (there is no layout positions yet calculated)
                InternalAddVertex(it, vc);
            }
            if (forceVisPropRecovery)
                ReapplyVertexVisualProperties();
            //assign graph
            LogicCore.Graph = graph;
        }
        #endregion

        #region RelayoutGraph()

        private bool IsCalculating;
        private object calcLocker = new object();

        private async Task _relayoutGraph(bool generateAllEdges = false, bool standalone = true)
        {
            //lock to GraphArea instance and check if calculation is already running
            lock (calcLocker)
            {
                //do not allow new calculation to begin until previous isn't finished
                if (IsCalculating) return;
                IsCalculating = true;
            }

            try
            {
                Dictionary<TVertex, Measure.Size> vertexSizes = null;
                IExternalLayout<TVertex> alg = null; //layout algorithm
                Dictionary<TVertex, Measure.Rect> rectangles = null; //rectangled size data
                IExternalOverlapRemoval<TVertex> overlap = null; //overlap removal algorithm
                IExternalEdgeRouting<TVertex, TEdge> eralg = null;

                var result = false;
                await DispatcherHelper.CheckBeginInvokeOnUi(new Action(() =>
                {
                    if (LogicCore == null)
                        throw new GX_InvalidDataException("LogicCore -> Not initialized!");
                    if (LogicCore.Graph == null)
                        throw new GX_InvalidDataException("LogicCore -> Graph property is not set!"); 
                    if (_vertexlist.Count == 0)
                        return; // no vertexes == no edges

                    UpdateLayout(); //update layout so we can get actual control sizes

                    if (LogicCore.AreVertexSizesNeeded())
                        vertexSizes = GetVertexSizes();
                    alg = LogicCore.GenerateLayoutAlgorithm(vertexSizes);
                    if (alg == null && !LogicCore.IsCustomLayout)
                    {
                        //await new MessageDialog("Layout type not supported yet!").ShowAsync();
                        Debug.Assert(false, "Layout type is not yet supported!");
                        return;
                    }

                    //setup overlap removal algorythm
                    if (LogicCore.AreOverlapNeeded())
                        overlap = LogicCore.GenerateOverlapRemovalAlgorithm(rectangles);

                    //setup Edge Routing algorithm
                    eralg = LogicCore.GenerateEdgeRoutingAlgorithm(DesiredSize.ToGraphX());
                    result = true;
                }));
                if (!result) return;

                IDictionary<TVertex, Measure.Point> resultCoords;
                if (alg != null)
                {
                    alg.Compute();
                    OnLayoutCalculationFinished();
                    //if (Worker != null) Worker.ReportProgress(33, 0);
                    //result data storage
                    resultCoords = alg.VertexPositions;
                } //get default coordinates if using Custom layout
                else
                {
                    //UpdateLayout();
                    resultCoords = GetVertexPositions();
                }

                //overlap removal
                if (overlap != null)
                {
                    //generate rectangle data from sizes
                    var coords = resultCoords;
                    UpdateLayout();
                    await DispatcherHelper.CheckBeginInvokeOnUi(() =>
                    {
                        rectangles = GetVertexSizeRectangles(coords, vertexSizes, true);
                    });
                    overlap.Rectangles = rectangles;
                    overlap.Compute();
                    OnOverlapRemovalCalculationFinished();
                    resultCoords = new Dictionary<TVertex, Measure.Point>();
                    foreach (var res in overlap.Rectangles)
                        resultCoords.Add(res.Key, new Measure.Point(res.Value.Left, res.Value.Top));
                    //if (Worker != null) Worker.ReportProgress(66, 1);
                }


                await DispatcherHelper.CheckBeginInvokeOnUi(() =>
                {
                    LogicCore.CreateNewAlgorithmStorage(alg, overlap, eralg);

                    if (MoveAnimation != null)
                    {
                        MoveAnimation.CleanupBaseData();
                        MoveAnimation.Cleanup();
                    }
                    //setup vertex positions from result data
                    foreach (var item in resultCoords)
                    {
                        if (!_vertexlist.ContainsKey(item.Key)) continue;
                        var vc = _vertexlist[item.Key];

                        SetFinalX(vc, item.Value.X);
                        SetFinalY(vc, item.Value.Y);

                        if (MoveAnimation == null || double.IsNaN(GetX(vc)))
                            vc.SetPosition(item.Value.X, item.Value.Y, false);
                        else MoveAnimation.AddVertexData(vc, item.Value);
                        vc.Visibility = Visibility.Visible; //show vertexes with layout positions assigned
                    }
                    if (MoveAnimation != null)
                    {
                        if (MoveAnimation.VertexStorage.Count > 0)
                            MoveAnimation.RunVertexAnimation();

                        
                        foreach (var item in _edgeslist.Values)
                            MoveAnimation.AddEdgeData(item);
                        if (MoveAnimation.EdgeStorage.Count > 0)
                            MoveAnimation.RunEdgeAnimation();
                        
                    }
                    UpdateLayout(); //need to update before edge routing
                });

                //Edge Routing
                if (eralg != null)
                {
                    await DispatcherHelper.CheckBeginInvokeOnUi(() =>
                    {
                        //var size = Parent is ZoomControl ? (Parent as ZoomControl).Presenter.ContentSize : DesiredSize;
                        eralg.AreaRectangle = ContentSize.ToGraphX(); // new Rect(TopLeft.X, TopLeft.Y, size.Width, size.Height);
                        rectangles = GetVertexSizeRectangles(resultCoords, vertexSizes);
                    });
                    eralg.VertexPositions = resultCoords;
                    eralg.VertexSizes = rectangles;
                    eralg.Compute();
                    OnEdgeRoutingCalculationFinished();
                    if (eralg.EdgeRoutes != null)
                        foreach (var item in eralg.EdgeRoutes)
                            item.Key.RoutingPoints = item.Value;
                    //if (Worker != null) Worker.ReportProgress(99, 1);
                    
                }
                await DispatcherHelper.CheckBeginInvokeOnUi(() =>
                {
                    //UpdateLayout();
                    MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    LogicCore.CreateNewAlgorithmStorage(alg, overlap, eralg);

                    if (generateAllEdges)
                    {
                        if (_edgeslist.Count == 0)
                        {
                            this.generateAllEdges();
                            if (EnableVisualPropsRecovery) ReapplyEdgeVisualProperties();
                        }
                        else UpdateAllEdges();
                    }
                    if (!standalone)
                    {
                        if (EnableVisualPropsRecovery) ReapplyVertexVisualProperties();
                        OnGenerateGraphFinished();
                    }
                    else OnRelayoutFinished();
                });
            }
            finally
            {
                IsCalculating = false;
            }
        }

        /// <summary>
        /// Relayout graph using the same vertexes
        /// </summary>
        /// <param name="generateAllEdges">Generate all available edges for graph</param>
        public void RelayoutGraph(bool generateAllEdges = false)
        {
            _relayoutGraphMain(generateAllEdges);
        }

        private void _relayoutGraphMain(bool generateAllEdges = false, bool standalone = true)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");

            if (LogicCore.AsyncAlgorithmCompute)
            {
                _relayoutGraph(generateAllEdges, standalone);
                return;
            }
            _relayoutGraph(generateAllEdges, standalone).Wait();
        }

        #endregion

        /// <summary>
        /// Generate visual graph
        /// </summary>
        /// <param name="graph">Data graph</param>
        /// <param name="generateAllEdges">Generate all available edges for graph</param>
        /// <param name="dataContextToDataItem">Sets visual edge and vertex controls DataContext property to vertex data item of the control (Allows prop binding in xaml templates)</param>
        public void GenerateGraph(TGraph graph, bool generateAllEdges = false, bool dataContextToDataItem = true)
        {
            if (AutoAssignMissingDataId)
                AutoresolveIds(graph);
            if(!LogicCore.IsCustomLayout)
                PreloadVertexes(graph, dataContextToDataItem);
            _relayoutGraphMain(generateAllEdges, false);
        }

        /// <summary>
        /// Generate visual graph using Graph property (it must be set before this method is called)
        /// </summary>
        /// <param name="generateAllEdges">Generate all available edges for graph</param>
        /// <param name="dataContextToDataItem">Sets visual edge and vertex controls DataContext property to vertex data item of the control (Allows prop binding in xaml templates)</param>
        public void GenerateGraph(bool generateAllEdges = false, bool dataContextToDataItem = true)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized! (Is NULL)");
            if (LogicCore.Graph == null)
                throw new InvalidDataException("GraphArea.GenerateGraph() -> LogicCore.Graph property is null while trying to generate graph!");
            GenerateGraph(LogicCore.Graph, generateAllEdges, dataContextToDataItem);
        }

        private void AutoresolveIds(TGraph graph = null)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            if (graph == null) graph = LogicCore.Graph;
            if (graph == null) return;
            _dataIdsCollection.Clear();
            _dataIdCounter = 1;
            var count = graph.Vertices.Count();
            for (var i = 0; i < count; i++)
            {
                var element = graph.Vertices.ElementAt(i);
                if (element.ID != -1 && !_dataIdsCollection.Contains(element.ID)) 
                    _dataIdsCollection.Add(element.ID);
            }
            foreach (var item in graph.Vertices.Where(a => a.ID == -1))
                item.ID = GetNextUniqueId();

            _dataIdsCollection.Clear();
            _dataIdCounter = 1;
            count = graph.Edges.Count();
            for (var i = 0; i < count; i++)
            {
                var element2 = graph.Edges.ElementAt(i);
                if (element2.ID != -1 && !_dataIdsCollection.Contains(element2.ID))
                    _dataIdsCollection.Add(element2.ID);
            }
            foreach (var item in graph.Edges.Where(a => a.ID == -1))
                item.ID = GetNextUniqueId();
        }
        #endregion 


        #region Methods for EDGE and VERTEX properties set

        private void ReapplyVertexVisualProperties()
        {
            foreach (var item in VertexList.Values)
                ReapplySingleVertexVisualProperties(item);
        }

        private bool? _svVertexLabelShow;

        private void ReapplySingleVertexVisualProperties(VertexControl item)
        {
            if (_svVerticesDragEnabled != null) DragBehaviour.SetIsDragEnabled(item, _svVerticesDragEnabled.Value);
            if (_svVerticesDragUpdateEdges != null) DragBehaviour.SetUpdateEdgesOnMove(item, _svVerticesDragUpdateEdges.Value);
            if (_svVertexShape != null) item.MathShape = _svVertexShape.Value;
            if (_svVertexLabelShow != null) item.ShowLabel = _svVertexLabelShow.Value;
            if (_svVertexHlEnabled != null) HighlightBehaviour.SetIsHighlightEnabled(item, _svVertexHlEnabled.Value);
            if (_svVertexHlObjectType != null) HighlightBehaviour.SetHighlightControl(item, _svVertexHlObjectType.Value);
            if (_svVertexHlEdgesType != null) HighlightBehaviour.SetHighlightEdges(item, _svVertexHlEdgesType.Value);
        }

        private void ReapplyEdgeVisualProperties()
        {
            foreach (var item in EdgesList.Values)
                ReapplySingleEdgeVisualProperties(item);
        }

        private void ReapplySingleEdgeVisualProperties(EdgeControl item)
        {
            if (_svEdgeDashStyle != null) item.DashStyle = _svEdgeDashStyle.Value;
            if (_svShowEdgeArrows != null) item.ShowArrows = _svShowEdgeArrows.Value;
            if (_svShowEdgeLabels != null) item.ShowLabel = _svShowEdgeLabels.Value;
            if (_svAlignEdgeLabels != null) item.AlignLabelsToEdges = _svAlignEdgeLabels.Value;
            if (_svUpdateLabelPosition != null) item.UpdateLabelPosition = _svUpdateLabelPosition.Value;
            if (_svEdgeHlEnabled != null) HighlightBehaviour.SetIsHighlightEnabled(item, _svEdgeHlEnabled.Value);
            if (_svEdgeHlObjectType != null) HighlightBehaviour.SetHighlightControl(item, _svEdgeHlObjectType.Value);
            HighlightBehaviour.SetHighlightEdges(item, EdgesType.All);
        }

        private bool? _svUpdateLabelPosition;
        public void UpdateEdgeLabelPosition(bool value)
        {
            _svUpdateLabelPosition = value;
            foreach (var item in EdgesList)
                item.Value.UpdateLabelPosition = value;
        }

        private EdgeDashStyle? _svEdgeDashStyle;// EdgeDashStyle.Solid;
        /// <summary>
        /// Sets all edges dash style
        /// </summary>
        /// <param name="style">Selected style</param>
        public void SetEdgesDashStyle(EdgeDashStyle style)
        {
            _svEdgeDashStyle = style;
            foreach (var item in EdgesList)
                item.Value.DashStyle = style;
        }

        private bool? _svShowEdgeArrows;
        /// <summary>
        /// Show or hide all edges arrows. Default value is True.
        /// </summary>
        /// <param name="isEnabled">Boolean value</param>
        public void ShowAllEdgesArrows(bool isEnabled = true)
        {
            _svShowEdgeArrows = isEnabled;
            foreach (var item in _edgeslist.Values)
                item.ShowArrows = isEnabled;
        }

        private bool? _svShowEdgeLabels;
        /// <summary>
        /// Show or hide all edges labels
        /// </summary>
        /// <param name="isEnabled">Boolean value</param>
        public void ShowAllEdgesLabels(bool isEnabled = true)
        {
            _svShowEdgeLabels = isEnabled;
            foreach (var item in _edgeslist.Values)
                item.ShowLabel = isEnabled;
            //InvalidateVisual();
        }

        /// <summary>
        /// Show or hide all vertex labels
        /// </summary>
        /// <param name="isEnabled">Boolean value</param>
        public void ShowAllVerticesLabels(bool isEnabled = true)
        {
            _svVertexLabelShow = isEnabled;
            foreach (var item in _vertexlist.Values)
                item.ShowLabel = isEnabled;
        }

        private bool? _svAlignEdgeLabels;
        /// <summary>
        /// Aligns all labels with edges or displays them horizontaly
        /// </summary>
        /// <param name="isEnabled">Boolean value</param>
        public void AlignAllEdgesLabels(bool isEnabled = true)
        {
            _svAlignEdgeLabels = isEnabled;
            foreach (var item in _edgeslist.Values)
            {
                item.AlignLabelsToEdges = isEnabled;
            }
            //InvalidateVisual();
        }

        private bool? _svVerticesDragEnabled;
        private bool? _svVerticesDragUpdateEdges;
        /// <summary>
        /// Sets drag mode for all vertices
        /// </summary>
        /// <param name="isEnabled">Is drag mode enabled</param>
        /// <param name="updateEdgesOnMove">Is edges update enabled while dragging (use this if you have edge routing algorithms enabled)</param>
        public void SetVerticesDrag(bool isEnabled, bool updateEdgesOnMove = false)
        {
            _svVerticesDragEnabled = isEnabled;
            _svVerticesDragUpdateEdges = updateEdgesOnMove;

            foreach (var item in VertexList)
            {
                DragBehaviour.SetIsDragEnabled(item.Value, isEnabled);
                DragBehaviour.SetUpdateEdgesOnMove(item.Value, updateEdgesOnMove);
            }
        }

        private VertexShape? _svVertexShape;// = VertexShape.Rectangle;
        /// <summary>
        /// Sets math shape for all vertices
        /// </summary>
        /// <param name="shape">Selected math shape</param>
        public void SetVerticesMathShape(VertexShape shape)
        {
            _svVertexShape = shape;
            foreach (var item in VertexList)
                item.Value.MathShape = shape;
        }

        private bool? _svVertexHlEnabled;
        private GraphControlType? _svVertexHlObjectType;// = GraphControlType.VertexAndEdge;
        private EdgesType? _svVertexHlEdgesType;// = EdgesType.All;
        /// <summary>
        /// Sets vertices highlight logic
        /// </summary>
        /// <param name="isEnabled">Is highlight enabled</param>
        /// <param name="hlObjectsOfType">Highlight connected objects if specified type</param>
        /// <param name="hlEdgesOfType">Highlight edges of specified type (according to previous property set)</param>
        public void SetVerticesHighlight(bool isEnabled, GraphControlType hlObjectsOfType, EdgesType hlEdgesOfType = EdgesType.All)
        {
            _svVertexHlEnabled = isEnabled;
            _svVertexHlObjectType = hlObjectsOfType;
            _svVertexHlEdgesType = hlEdgesOfType;
            foreach (var item in VertexList)
            {
                HighlightBehaviour.SetHighlighted(item.Value, false);
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, isEnabled);
                HighlightBehaviour.SetHighlightControl(item.Value, hlObjectsOfType);
                HighlightBehaviour.SetHighlightEdges(item.Value, hlEdgesOfType);
            }
        }

        private bool? _svEdgeHlEnabled;
        private GraphControlType? _svEdgeHlObjectType;// = GraphControlType.VertexAndEdge;
        /// <summary>
        /// Sets edges highlight logic
        /// </summary>
        /// <param name="isEnabled">Is highlight enabled</param>
        /// <param name="hlObjectsOfType">Highlight connected objects if specified type</param>
        public void SetEdgesHighlight(bool isEnabled, GraphControlType hlObjectsOfType)
        {
            _svEdgeHlEnabled = isEnabled;
            _svEdgeHlObjectType = hlObjectsOfType;

            foreach (var item in VertexList)
            {
                HighlightBehaviour.SetHighlighted(item.Value, false);
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, isEnabled);
                HighlightBehaviour.SetHighlightControl(item.Value, hlObjectsOfType);
                HighlightBehaviour.SetHighlightEdges(item.Value, EdgesType.All);
            }
        }

        #endregion

        #region Generate Edges (ForVertex, All ... and stuff)

        #region ComputeEdgeRoutesByVertex()
        /// <summary>
        /// Compute new edge routes for all edges of the vertex
        /// </summary>
        /// <param name="vc">Vertex visual control</param>
        /// <param name="vertexDataNeedUpdate">If vertex data inside edge routing algorthm needs to be updated</param>
        internal override void ComputeEdgeRoutesByVertex(VertexControl vc, bool vertexDataNeedUpdate = true)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore is not initialized!");
            LogicCore.ComputeEdgeRoutesByVertex((TVertex)vc.Vertex, vertexDataNeedUpdate ? (Measure.Point?)vc.GetPositionGraphX() : null, vertexDataNeedUpdate ? (Measure.Size?)new Measure.Size(vc.ActualWidth, vc.ActualHeight) : null);
        }
        #endregion

        #region GenerateAllEdges()

        private void generateAllEdges(Visibility defaultVisibility = Visibility.Visible)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            RemoveAllEdges();
            foreach (var item in LogicCore.Graph.Edges)
            {
                if (item.Source == null || item.Target == null) continue;
                if (!_vertexlist.ContainsKey(item.Source) || !_vertexlist.ContainsKey(item.Target)) continue;
                var edgectrl = ControlFactory.CreateEdgeControl(_vertexlist[item.Source], _vertexlist[item.Target],
                                                                    item, false, true, defaultVisibility);
                InternalInsertEdge(item, edgectrl);
                //setup path
                if (_svShowEdgeLabels == true)
                    edgectrl.ShowLabel = true;
                edgectrl.PrepareEdgePath();
                //edgectrl.InvalidateChildren();
            }
            //this.InvalidateVisual();

            if (LogicCore.EnableParallelEdges)
                ParallelizeEdges();
            if (_svShowEdgeLabels == true && LogicCore.EnableEdgeLabelsOverlapRemoval)
               RemoveEdgeLabelsOverlap();

        }

        private void RemoveEdgeLabelsOverlap()
        {
            var sz = GetVertexSizeRectangles();

            var sizes = sz.ToDictionary(item => new LabelOverlapData() {Id = item.Key.ID, IsVertex = true}, item => item.Value);
            foreach (var item in EdgesList)
            {
                item.Value.UpdateLabelLayout();
                sizes.Add(new LabelOverlapData() { Id = item.Key.ID, IsVertex = false }, item.Value.GetLabelSize());
            }
  
            var orAlgo = LogicCore.AlgorithmFactory.CreateFSAA(sizes, 15f, 15f);
            orAlgo.Compute();
            foreach (var item in orAlgo.Rectangles)
            {
                if (item.Key.IsVertex)
                {
                    var vertex = VertexList.FirstOrDefault(a => a.Key.ID == item.Key.Id).Value;
                    if (vertex == null) throw new GX_InvalidDataException("RemoveEdgeLabelsOverlap() -> Vertex not found!");
                    vertex.SetPosition(item.Value.X + item.Value.Width * .5, item.Value.Y + item.Value.Height * .5);
                    //vertex.Arrange(item.Value);
                }
                else
                {
                    var edge = EdgesList.FirstOrDefault(a => a.Key.ID == item.Key.Id).Value;
                    if (edge == null) throw new GX_InvalidDataException("RemoveEdgeLabelsOverlap() -> Edge not found!");
                    edge.SetCustomLabelSize(item.Value.ToWindows());
                }
            }
            //recalculate route path for new vertex positions
            if (LogicCore.AlgorithmStorage.EdgeRouting != null)
            {
                LogicCore.AlgorithmStorage.EdgeRouting.VertexSizes = GetVertexSizeRectangles();
                LogicCore.AlgorithmStorage.EdgeRouting.VertexPositions = GetVertexPositions();
                LogicCore.AlgorithmStorage.EdgeRouting.Compute();
                if (LogicCore.AlgorithmStorage.EdgeRouting.EdgeRoutes != null)
                    foreach (var item in LogicCore.AlgorithmStorage.EdgeRouting.EdgeRoutes)
                        item.Key.RoutingPoints = item.Value;
            }
            foreach (var item in EdgesList)
            {
                item.Value.PrepareEdgePath(false, null, false);
            }
            //update edges
           // UpdateAllEdges();
        }

        private class LabelOverlapData
        {
            public bool IsVertex;
            public int Id;
        }

        /// <summary>
        /// Generates all possible valid edges for Graph vertexes
        /// </summary>
        /// <param name="defaultVisibility">Default edge visibility on layout</param>
        /// <param name="updateLayout">Ensures that layout is properly updated before edges calculation. If you are sure that it is already updated you can set this param to False to increase performance. </param>
        public void GenerateAllEdges(Visibility defaultVisibility = Visibility.Visible, bool updateLayout = true)
        {
            if(updateLayout) UpdateLayout();
            generateAllEdges(defaultVisibility);
        }

        private void ParallelizeEdges()
        {
            var usedIds = _edgeslist.Count > 20 ? new HashSet<int>() as ICollection<int> : new List<int>();

            foreach (var item in EdgesList)
            {
                if (usedIds.Contains(item.Key.ID) || !item.Value.CanBeParallel) continue;
                var list = new List<EdgeControl> {item.Value};
                //that list will contain checks for edges that goes form target to source
                var cList = new List<bool> {false};
                foreach (var edge in EdgesList)
                {
                    //skip the same edge
                    if (item.Key.ID == edge.Key.ID) continue;
                    //add source to target edge
                    if (edge.Value.CanBeParallel && ((item.Key.Source.ID == edge.Key.Source.ID && item.Key.Target.ID == edge.Key.Target.ID)))
                    {
                        list.Add(edge.Value);
                        cList.Add(false);
                    }
                    //add target to source edge and remember the check
                    if (item.Key.Source.ID == edge.Key.Target.ID && item.Key.Target.ID == edge.Key.Source.ID)
                    {
                        cList.Add(true);
                        list.Add(edge.Value);
                    }
                    //else cList.Add(false);
                }

                //do stuff
                if (list.Count > 1)
                {
                    //trigger to show in which side to step distance
                    bool viceversa = false;
                    //check if total number of edges is even or not
                    bool even = (list.Count % 2) == 0;
                    //get the resulting step distance for the case
                    int distance = even ? (int)(LogicCore.ParallelEdgeDistance * .5) : LogicCore.ParallelEdgeDistance;

                    //leave first edge intact if we have not even edges count
                    for (int i = even ? 0 : 1; i < list.Count; i++)
                    {
                        //var dist = ParallelEdgeDistance;
                        //if (chet && i < 2) dist = distance;
                        //if (cList[i] && ((!chet && !prevc) || list.Count == 2)) viceversa = !viceversa;
                        //if source to target edge
                        if (!cList[i])
                        {
                            list[i].SourceOffset = (viceversa ? distance : -distance) * (1 + ((even ? i : i - 1) / 2));
                            list[i].TargetOffset = -list[i].SourceOffset;
                        }
                        else //if target to source edge - just switch offsets
                        {
                            list[i].TargetOffset = (viceversa ? distance : -distance) * (1 + ((even ? i : i - 1) / 2));
                            list[i].SourceOffset = -list[i].TargetOffset;
                        }
                        //change trigger to opposite
                        viceversa = !viceversa;
                    }
                }

                //remember used edges IDs
                foreach (var item2 in list)
                {
                    var edge = item2.Edge as TEdge;
                    if (edge != null) usedIds.Add(edge.ID);
                }
                list.Clear();
            }
        }



        #endregion

        #region GenerateEdgesForVertex()
        /// <summary>
        /// Generates and displays edges for specified vertex
        /// </summary>
        /// <param name="vc">Vertex control</param>
        /// <param name="edgeType">Type of edges to display</param>
        /// <param name="defaultVisibility">Default edge visibility on layout</param>
        public override void GenerateEdgesForVertex(VertexControl vc, EdgesType edgeType, Visibility defaultVisibility = Visibility.Visible)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            RemoveAllEdges();

            TEdge[] inlist = null;
            TEdge[] outlist = null;
            switch (edgeType)
            {
                case EdgesType.Out:
                    outlist = LogicCore.Graph.OutEdges(vc.Vertex as TVertex).ToArray();
                    break;
                case EdgesType.In:
                    inlist = LogicCore.Graph.InEdges(vc.Vertex as TVertex).ToArray();
                    break;
                default:
                    outlist = LogicCore.Graph.OutEdges(vc.Vertex as TVertex).ToArray();
                    inlist = LogicCore.Graph.InEdges(vc.Vertex as TVertex).ToArray();
                    break;
            }
            bool gotSelfLoop = false;
            if (inlist != null)
                foreach (var item in inlist)
                {
					if(gotSelfLoop) continue;
                    var ctrl = ControlFactory.CreateEdgeControl(_vertexlist[item.Source], vc, item, false, true,
                                                                     defaultVisibility);                   
                    InsertEdge(item, ctrl);
                    ctrl.PrepareEdgePath();
                    if(item.Source == item.Target) gotSelfLoop = true;
                }
            if (outlist != null)
                foreach (var item in outlist)
                {
					if(gotSelfLoop) continue;                    
                    var ctrl = ControlFactory.CreateEdgeControl(vc, _vertexlist[item.Target], item, false, true,
                                                 defaultVisibility);
                    InsertEdge(item, ctrl);
                    ctrl.PrepareEdgePath();
                    if(item.Source == item.Target) gotSelfLoop = true;
                }
        }
        #endregion

        /// <summary>
        /// Update visual appearance for all possible visual edges
        /// </summary>
        public void UpdateAllEdges()
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            foreach (var ec in _edgeslist.Values)
            {
                ec.PrepareEdgePath();
                //ec.InvalidateVisual();
                ec.InvalidateChildren();
            }
            if (LogicCore.EnableParallelEdges)
                ParallelizeEdges();
        }

        

        #endregion

        #region GetRelatedControls
        /// <summary>
        /// Get controls related to specified control 
        /// </summary>
        /// <param name="ctrl">Original control</param>
        /// <param name="resultType">Type of resulting related controls</param>
        /// <param name="edgesType">Optional edge controls type</param>
        public override List<IGraphControl> GetRelatedControls(IGraphControl ctrl, GraphControlType resultType = GraphControlType.VertexAndEdge, EdgesType edgesType = EdgesType.Out)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            if(LogicCore.Graph == null) 
            {
                Debug.WriteLine("LogicCore.Graph property not set while using GetRelatedControls method!");
                return null;
            }

            var list = new List<IGraphControl>();
            var vc = ctrl as VertexControl;
            if (vc != null)
            {
                //if (vc.Vertex == null) return null;
                List<TEdge> edgesInList = null;
                List<TEdge> edgesOutList = null;
                if (edgesType == EdgesType.In || edgesType == EdgesType.All)
                {
                    IEnumerable<TEdge> inEdges;
                    LogicCore.Graph.TryGetInEdges(vc.Vertex as TVertex, out inEdges);
                    edgesInList = inEdges == null? null : inEdges.ToList();
                }

                if (edgesType == EdgesType.Out || edgesType == EdgesType.All)
                {
                    IEnumerable<TEdge> outEdges;
                    LogicCore.Graph.TryGetOutEdges(vc.Vertex as TVertex, out outEdges);
                    edgesOutList = outEdges == null ? null : outEdges.ToList();
                }

                if (resultType == GraphControlType.Edge || resultType == GraphControlType.VertexAndEdge)
                {
                    if (edgesInList != null)
                        list.AddRange((from item in edgesInList where _edgeslist.ContainsKey(item) select _edgeslist[item]));
                    if (edgesOutList != null)
                        list.AddRange((from item in edgesOutList where _edgeslist.ContainsKey(item) select _edgeslist[item]));
                }
                if (resultType != GraphControlType.Vertex && resultType != GraphControlType.VertexAndEdge) return list;

                if (edgesInList != null)
                    list.AddRange((from item in edgesInList where _vertexlist.ContainsKey(item.Source) select _vertexlist[item.Source]));
                if (edgesOutList != null)
                    list.AddRange((from item in edgesOutList where _vertexlist.ContainsKey(item.Target) select _vertexlist[item.Target]));
                return list;
            }
            var ec = ctrl as EdgeControl;
            if (ctrl == null || ec == null) return list;
            var edge = (TEdge)ec.Edge;
            if (resultType == GraphControlType.Edge) return list;
            if (_vertexlist.ContainsKey(edge.Target)) list.Add(_vertexlist[edge.Target]);
            if (_vertexlist.ContainsKey(edge.Source)) list.Add(_vertexlist[edge.Source]);
            return list;
        }
        #endregion

        #region Save

        public void SerializeToFile(string filename)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            if (LogicCore.FileServiceProvider == null)
                throw new GX_ObjectNotFoundException("LogicCore::FileServiceProvider must be set before using file operation methods!");

            if (AutoAssignMissingDataId)
            {
                AutoresolveIds();
            }

            var dlist = new List<GraphSerializationData>();
            foreach (var item in VertexList) //ALWAYS serialize vertices first
            {
                dlist.Add(new GraphSerializationData { Position = item.Value.GetPositionGraphX(), Data = item.Key });
                if (item.Key.ID == -1) throw new GX_InvalidDataException("SerializeToFile() -> All vertex datas must have positive unique ID!");
            }
            foreach (var item in EdgesList)
            {
               // item.Key.RoutingPoints = new Point[] { new Point(0, 123), new Point(12, 12), new Point(10, 234.5) };
                dlist.Add(new GraphSerializationData { Position = new Measure.Point(), Data = item.Key });
                if (item.Key.ID == -1) throw new GX_InvalidDataException("SerializeToFile() -> All edge datas must have positive unique ID!");
            }

            LogicCore.FileServiceProvider.SerializeDataToFile(filename, dlist);

        }

        public void DeserializeFromFile(string filename)
        {
            if (LogicCore == null)
                throw new GX_InvalidDataException("LogicCore -> Not initialized!");
            if (LogicCore.FileServiceProvider == null)
                throw new GX_ObjectNotFoundException("LogicCore::FileServiceProvider must be set before using file operation methods!");

            RemoveAllEdges();
            RemoveAllVertices();

            var data = LogicCore.FileServiceProvider.DeserializeDataFromFile(filename);

            if (LogicCore.Graph == null) LogicCore.Graph = Activator.CreateInstance<TGraph>();                
            else LogicCore.Graph.Clear();

            var vlist = data.Where(a => a.Data is TVertex);
            foreach (var item in vlist)
            {
                var vertexdata = item.Data as TVertex;
                var ctrl = ControlFactory.CreateVertexControl(vertexdata);
                ctrl.SetPosition(item.Position.X, item.Position.Y);
                AddVertex(vertexdata, ctrl);
                LogicCore.Graph.AddVertex(vertexdata);
            }
            var elist = data.Where(a => a.Data is TEdge);

            foreach (var item in elist)
            {
                var edgedata = item.Data as TEdge;
                if (edgedata == null) continue;
                var sourceid = edgedata.Source.ID; var targetid = edgedata.Target.ID;
                var datasource = _vertexlist.Keys.FirstOrDefault(a => a.ID == sourceid); var datatarget = _vertexlist.Keys.FirstOrDefault(a => a.ID == targetid);

                edgedata.Source = datasource;
                edgedata.Target = datatarget;

                if (datasource == null || datatarget == null)
                    throw new GX_SerializationException("DeserializeFromFile() -> Serialization logic is broken! Vertex not found. All vertices must be processed before edges!");
                var ecc = new EdgeControl { Edge = edgedata, Source = _vertexlist[datasource], Target = _vertexlist[datatarget] };
                InsertEdge(edgedata, ecc);
                LogicCore.Graph.AddEdge(edgedata);
            }
            //update edge layout and shapes manually
            //to correctly draw arrows in any case except they are manually disabled
            UpdateLayout();
            //TODO
            //foreach (var item in EdgesList.Values)
            //   item.OnApplyTemplate();
        }

        #endregion

        #region Export and printing

        /// <summary>
        /// Export current graph layout into the PNG image file. layout will be saved in full size.
        /// </summary>
        public void ExportAsPng()
        {
            ExportAsImage(ImageType.PNG);
        }

        /// <summary>
        /// Export current graph layout into the JPEG image file. layout will be saved in full size.
        /// </summary>
        /// <param name="quality">Optional image quality parameter</param>   
        public void ExportAsJpeg(int quality = 100)
        {
            ExportAsImage(ImageType.JPEG, true, PrintHelper.DefaultDPI, quality);
        }

        /// <summary>
        /// Export current graph layout into the chosen image file and format. layout will be saved in full size.
        /// </summary>
        /// <param name="itype">Image format</param>
        /// <param name="dpi">Optional image DPI parameter</param>
        /// <param name="useZoomControlSurface">Use zoom control parent surface to render bitmap (only visible zoom content will be exported)</param>
        /// <param name="quality">Optional image quality parameter (for JPEG)</param>   
        public void ExportAsImage(ImageType itype, bool useZoomControlSurface = true, double dpi = PrintHelper.DefaultDPI, int quality = 100)
        {
            string fileExt;
            string fileType = itype.ToString();
            switch (itype)
            {
                case ImageType.PNG: fileExt = "*.png";
                    break;
                case ImageType.JPEG: fileExt = "*.jpg";
                    break;
                case ImageType.BMP: fileExt = "*.bmp";
                    break;
                case ImageType.GIF: fileExt = "*.gif";
                    break;
                case ImageType.TIFF: fileExt = "*.tiff";
                    break;
                default: throw new GX_InvalidDataException("ExportAsImage() -> Unknown output image format specified!");
            }
            //TODO dialog
            /*var dlg = new SaveFileDialog { Filter = String.Format("{0} Image File ({1})|{1}", fileType, fileExt), Title = String.Format("Exporting graph as {0} image...", fileType) };
            if (dlg.ShowDialog() == true)
            {
                PrintHelper.ExportToImage(this, new Uri(dlg.FileName), itype, true, dpi, quality);
            }*/
        }

       /* public Bitmap ExportToBitmap(double dpi = PrintHelper.DefaultDPI)
        {
            return PrintHelper.RenderTargetBitmapToBitmap(PrintHelper.RenderTargetBitmap(this, true, dpi));
        }*/

        
        /// <summary>
        /// Print current visual graph layout
        /// </summary>
        /// <param name="description">Optional header description</param>
        public void PrintDialog(string description = "")
        {
            PrintHelper.ShowPrintPreview(this, description);
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (StateStorage != null)
            {
                StateStorage.Dispose();
                StateStorage = null;
            }
            if (LogicCore != null)
            {
                LogicCore.Dispose();
                LogicCore = null;
            }
            MoveAnimation = null;
            DeleteAnimation = null;
            MouseOverAnimation = null;
        }

        /// <summary>
        /// Clear graph visual layout (all edges and vertices)
        /// </summary>
        /// <param name="removeCustomObjects">Also remove any possible custom objects</param>
        public void ClearLayout(bool removeCustomObjects = true)
        {
            RemoveAllEdges();
            RemoveAllVertices();
            if(removeCustomObjects)
                base.Children.Clear();
        }
        #endregion
    }
}
