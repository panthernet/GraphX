using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.Measure;
using GraphX.PCL.Common;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Models
{
    public partial class GXLogicCore<TVertex, TEdge, TGraph>: IGXLogicCore<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>, new()
    {
        #region Properties
        #region AlgoithmFactory
        /// <summary>
        /// Get an algorithm factory that provides different algorithm creation methods
        /// </summary>
        public IAlgorithmFactory<TVertex, TEdge, TGraph> AlgorithmFactory { get; private set; }

        #endregion

        #region AlgorithmStorage
        /// <summary>
        /// Gets or sets algorithm storage that contain all currently defined algorithm objects by type (default or external)
        /// Actual storage data is vital for correct edge routing operation after graph was regenerated.
        /// </summary>
        public IAlgorithmStorage<TVertex, TEdge> AlgorithmStorage { get; set; }
        #endregion

        /// <summary>
        /// Source vertex positions internal storage
        /// </summary>
        private IDictionary<TVertex, Point> _vertexPosSource;

        /// <summary>
        /// Source vertex sizes
        /// </summary>
        private Dictionary<TVertex, Size> _vertexSizes;

        /// <summary>
        /// Gets or sets if if edge label overlap removal enabled
        /// </summary>
        private bool EnableEdgeLabelsOverlapRemoval { get; set; }

        /// <summary>
        /// Gets is LayoutAlgorithmTypeEnum.Custom (NOT external) layout selected and used. Custom layout used to manualy generate graph.
        /// </summary>
        public bool IsCustomLayout { get { return DefaultLayoutAlgorithm == LayoutAlgorithmTypeEnum.Custom && ExternalLayoutAlgorithm == null; } }
        
        /// <summary>
        /// Gets or sets external layout algorithm that will be used instead of the default one.
        /// Negates DefaultLayoutAlgorithm property value if set.
        /// </summary>
        public IExternalLayout<TVertex, TEdge> ExternalLayoutAlgorithm { get; set; }
        
        /// <summary>
        /// Gets or sets external overlap removal algorithm that will be used instead of the default one.
        /// Negates DefaultOverlapRemovalAlgorithm property value if set.
        /// </summary>
        public IExternalOverlapRemoval<TVertex> ExternalOverlapRemovalAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets external edge routing algorithm that will be used instead of the default one.
        /// Negates DefaultEdgeRoutingAlgorithm property value if set.
        /// </summary>
        public IExternalEdgeRouting<TVertex, TEdge> ExternalEdgeRoutingAlgorithm { get; set; }

        private LayoutAlgorithmTypeEnum _defaultLayoutAlgorithm;
        /// <summary>
        /// Gets or sets default layout algorithm that will be used on graph generation/relayouting
        /// </summary>
        public LayoutAlgorithmTypeEnum DefaultLayoutAlgorithm { 
            get { return _defaultLayoutAlgorithm; } set { _defaultLayoutAlgorithm = value; SetDefaultParams(0); } 
        }

        private OverlapRemovalAlgorithmTypeEnum _defaultOverlapRemovalAlgorithm;
        /// <summary>
        /// Gets or sets default overlap removal algorithm that will be used on graph generation/relayouting
        /// </summary>
        public OverlapRemovalAlgorithmTypeEnum DefaultOverlapRemovalAlgorithm {
            get { return _defaultOverlapRemovalAlgorithm; }
            set { _defaultOverlapRemovalAlgorithm = value; SetDefaultParams(1);}
        }

        private EdgeRoutingAlgorithmTypeEnum _defaultEdgeRoutingAlgorithm;
        /// <summary>
        /// Gets or sets default edge routing algorithm that will be used on graph generation/relayouting
        /// </summary>
        public EdgeRoutingAlgorithmTypeEnum DefaultEdgeRoutingAlgorithm { 
            get { return _defaultEdgeRoutingAlgorithm; } set { _defaultEdgeRoutingAlgorithm = value; SetDefaultParams(2); } }

        /// <summary>
        /// Gets or sets default layout algorithm parameters that will be used on graph generation/relayouting
        /// </summary>
        public ILayoutParameters DefaultLayoutAlgorithmParams { get; set; }

        /// <summary>
        /// Gets or sets default overlap removal algorithm parameters that will be used on graph generation/relayouting
        /// </summary>
        public IOverlapRemovalParameters DefaultOverlapRemovalAlgorithmParams { get; set; }

        /// <summary>
        /// Gets or sets default edge routing algorithm parameters that will be used on graph generation/relayouting
        /// </summary>
        public IEdgeRoutingParameters DefaultEdgeRoutingAlgorithmParams { get; set; }

        /// <summary>
        /// Gets or sets if async algorithm computations are enabled
        /// </summary>
        public bool AsyncAlgorithmCompute { get; set; }

        /// <summary>
        /// Gets is graph has been filtered or not
        /// </summary>
        public bool IsFiltered { get; protected set; }

        public bool IsFilterRemoved { get; set; }

        /// <summary>
        /// Clear LogicCore data
        /// </summary>
        /// <param name="clearStorages">Also clear storages data</param>
        public void Clear(bool clearStorages = true)
        {
            _graph?.Clear();
            OriginalGraph?.Clear();
            if (clearStorages)
                CreateNewAlgorithmStorage(null, null, null);
        }

        /// <summary>
        /// Execute filters on the current logic core graph. Can be undone by PopFilters() or can be made permanent by ApplyFilters().
        /// </summary>
        public void PushFilters()
        {
            int i = 0;
            IsFilterRemoved = false;
            //remember original graph if we're about to start filtering
            if (Filters.Count > 0 && !IsFiltered)
                OriginalGraph = Graph.CopyToGraph<TGraph, TVertex, TEdge>();
            //reset graph if we remove filtering
            else if(Filters.Count == 0 && IsFiltered)
                PopFilters();
            while (Filters.Count > 0)
            {
                //start applying filter on original graph copy on the 1st pass and then on Graph property each other pass
                Graph = Filters.Dequeue().ProcessFilter( i==0 ? OriginalGraph.CopyToGraph<TGraph, TVertex, TEdge>() : Graph);
                i++;
                IsFiltered = true;
            }
        }

        /// <summary>
        /// Assign filtered graph to original one making current filter output permanent and unrevertable
        /// </summary>
        public void ApplyFilters()
        {
            if (!IsFiltered) return;
            OriginalGraph = Graph;
            Filters.Clear();
            IsFilterRemoved = true;
            IsFiltered = false;
        }

        /// <summary>
        /// Restores original graph to the state before any filters were applied
        /// </summary>
        public void PopFilters()
        {
            if (!IsFiltered) return;
            Filters.Clear();
            Graph = OriginalGraph;
            IsFilterRemoved = true;
            IsFiltered = false;        
        }

        /// <summary>
        /// Represents graph filters queue (FIFO)
        /// </summary>
        public Queue<IGraphFilter<TVertex, TEdge, TGraph>> Filters { get; set; } =
            new Queue<IGraphFilter<TVertex, TEdge, TGraph>>();

        /// <summary>
        /// Create default params if algorithm was changed and default params property is null
        /// </summary>
        /// <param name="type">Algorithm type (inner)</param>
        private void SetDefaultParams(int type)
        {
            switch (type)
            {
                    //layout
                case 0:
                    if(DefaultLayoutAlgorithmParams!=null) return;
                    DefaultLayoutAlgorithmParams = AlgorithmFactory.CreateLayoutParameters(DefaultLayoutAlgorithm);
                    return;
                    //overlap
                case 1:
                    if(DefaultOverlapRemovalAlgorithmParams!=null) return;
                    DefaultOverlapRemovalAlgorithmParams = AlgorithmFactory.CreateOverlapRemovalParameters(DefaultOverlapRemovalAlgorithm);
                    return;
                    //edge
                case 2:
                    if(DefaultEdgeRoutingAlgorithmParams!=null) return;
                    DefaultEdgeRoutingAlgorithmParams = AlgorithmFactory.CreateEdgeRoutingParameters(DefaultEdgeRoutingAlgorithm);
                    return;
            }
        }

        /// <summary>
        /// Gets or sets if edge curving technique enabled for smoother edges. Default value is True.
        /// </summary>
        public bool EdgeCurvingEnabled { get; set; }

        /// <summary>
        /// Gets or sets roughly the length of each line segment in the polyline
        /// approximation to a continuous curve in WPF units.  The smaller the
        /// number the smoother the curve, but slower the performance. Default is 8.
        /// </summary>
        public double EdgeCurvingTolerance { get; set; }

        private TGraph _graph;
        /// <summary>
        /// Gets or sets main graph object. Updating this property also updates OriginalGraph property.
        /// </summary>
        public TGraph Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        /// <summary>
        /// Gets unmodified graph before any filters has been applied
        /// </summary>
        public TGraph OriginalGraph { get; protected set; }

        /// <summary>
        /// Gets or sets if parallel edges are enabled. All edges between the same nodes will be separated by ParallelEdgeDistance value.
        /// This is post-process procedure and it may be performance-costly.
        /// </summary>
        public bool EnableParallelEdges { get; set; }

        /// <summary>
        /// Gets or sets distance by which edges are parallelized if EnableParallelEdges is true. Default value is 5.
        /// </summary>
        public int ParallelEdgeDistance { get; set; }

        #region IsEdgeRoutingEnabled
        /// <summary>
        /// Gets if edge routing will be performed on Compute() method execution
        /// </summary>
        public bool IsEdgeRoutingEnabled
        {
            get
            {
                return (ExternalEdgeRoutingAlgorithm == null && DefaultEdgeRoutingAlgorithm != EdgeRoutingAlgorithmTypeEnum.None) || ExternalEdgeRoutingAlgorithm != null;
            }
        }
        #endregion
        #endregion

        public GXLogicCore(TGraph graph)
        {
            CreateNewAlgorithmFactory();
            CreateNewAlgorithmStorage(null, null, null);
            Graph = graph;
            EdgeCurvingEnabled = true;
            EdgeCurvingTolerance = 8;
            ParallelEdgeDistance = 5;
        }

        public GXLogicCore()
            : this((TGraph)Activator.CreateInstance(typeof(TGraph)))
        {
        }

        public void Dispose()
        {
            Graph = null;
            ExternalEdgeRoutingAlgorithm = null;
            ExternalLayoutAlgorithm = null;
            ExternalOverlapRemovalAlgorithm = null;
            AlgorithmFactory = null;
            AlgorithmStorage = null;
            _vertexPosSource?.Clear();
        }


        /// <summary>
        /// Creates new algorithm factory
        /// </summary>
        public void CreateNewAlgorithmFactory()
        {
            AlgorithmFactory = new AlgorithmFactory<TVertex, TEdge, TGraph>();
        }

        /// <summary>
        /// Creates new algorithm storage
        /// </summary>
        /// <param name="layout">Layout algorithm</param>
        /// <param name="or">Overlap removal algorithm</param>
        /// <param name="er">Edge routing algorithm</param>
        public void CreateNewAlgorithmStorage(IExternalLayout<TVertex, TEdge> layout, IExternalOverlapRemoval<TVertex> or, IExternalEdgeRouting<TVertex, TEdge> er)
        {
            AlgorithmStorage = new AlgorithmStorage<TVertex, TEdge>(layout, or, er);
        }

        /// <summary>
        /// Creates algorithms by values in LogicCore properties and generates new AlgorithmStorage object
        /// </summary>
        /// <param name="vertexSizes">Vertex sizes</param>
        /// <param name="vertexPositions">Vertex positions</param>
        public bool GenerateAlgorithmStorage(Dictionary<TVertex, Size> vertexSizes,
            IDictionary<TVertex, Point> vertexPositions)
        {
            var algLay = GenerateLayoutAlgorithm(vertexSizes, vertexPositions);
            IExternalOverlapRemoval<TVertex> algOverlap = null;

            //TODO maybe rewise due to extensive memory consumption
            _vertexPosSource = vertexPositions;
            _vertexSizes = vertexSizes;
            Dictionary<TVertex, Rect> vertexRectangles = null;
            if(_vertexSizes != null)
                vertexRectangles = GetVertexSizeRectangles(vertexPositions, vertexSizes);

            //setup overlap removal algorythm
            if (AreOverlapNeeded())
                algOverlap = GenerateOverlapRemovalAlgorithm(vertexRectangles);
            var algEr = GenerateEdgeRoutingAlgorithm(CalculateContentRectangle().Size, vertexPositions, vertexRectangles);

            CreateNewAlgorithmStorage(algLay, algOverlap, algEr);
            return (AlgorithmStorage.Layout != null && (vertexSizes == null || vertexSizes.Count != 0)) || IsCustomLayout;
        }

        private void UpdateVertexDataForEr(TVertex vertexData, Point position, Size size)
        {
            AlgorithmStorage.EdgeRouting.UpdateVertexData(vertexData, position, new Rect(position, size));
        }

        /// <summary>
        /// Get visual vertex size rectangles (can be used by some algorithms)
        /// </summary>
        /// <param name="positions">Vertex positions collection</param>
        /// <param name="vertexSizes">Vertex sizes collection</param>
        /// <param name="getCenterPoints">True if you want center points returned instead of top-left (needed by overlap removal algo). Default value is False.</param>
        public Dictionary<TVertex, Rect> GetVertexSizeRectangles(IDictionary<TVertex, Point> positions, Dictionary<TVertex, Size> vertexSizes, bool getCenterPoints = false)
        {
            if (vertexSizes == null || positions == null)
                throw new GX_InvalidDataException("GetVertexSizeRectangles() -> Vertex sizes or positions not set!");
            var rectangles = new Dictionary<TVertex, Rect>();
            foreach (var vertex in _graph.Vertices.Where(a => a.SkipProcessing != ProcessingOptionEnum.Exclude))
            {
                Point position; Size size;
                if (!positions.TryGetValue(vertex, out position) || !vertexSizes.TryGetValue(vertex, out size)) continue;
                if (!getCenterPoints) rectangles[vertex] = new Rect(position.X, position.Y, size.Width, size.Height);
                else rectangles[vertex] = new Rect(position.X - size.Width * (float)0.5, position.Y - size.Height * (float)0.5, size.Width, size.Height);
            }
            return rectangles;
        }
    }
}
