using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Common.Models;
using QuickGraph;

namespace GraphX.PCL.Logic.Models
{
    public partial class GXLogicCore<TVertex, TEdge, TGraph>: IGXLogicCore<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
    {
        #region Properties
        #region AlgoithmFactory
        /// <summary>
        /// Provides different algorithm creation methods
        /// </summary>
        public IAlgorithmFactory<TVertex, TEdge, TGraph> AlgorithmFactory { get; private set; }

        #endregion

        #region AlgorithmStorage
        /// <summary>
        /// Get algorithm storage that contain all currently defined algorithms by type (default or external)
        /// </summary>
        public IAlgorithmStorage<TVertex, TEdge> AlgorithmStorage { get; private set; }
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
        public bool EnableEdgeLabelsOverlapRemoval { get; set; }

        /// <summary>
        /// Gets is custom layout selected and used
        /// </summary>
        public bool IsCustomLayout { get { return DefaultLayoutAlgorithm == LayoutAlgorithmTypeEnum.Custom && ExternalLayoutAlgorithm == null; } }

        public IExternalLayout<TVertex> ExternalLayoutAlgorithm { get; set; }
        public IExternalOverlapRemoval<TVertex> ExternalOverlapRemovalAlgorithm { get; set; }
        public IExternalEdgeRouting<TVertex, TEdge> ExternalEdgeRoutingAlgorithm { get; set; }

        private LayoutAlgorithmTypeEnum _defaultLayoutAlgorithm;
        public LayoutAlgorithmTypeEnum DefaultLayoutAlgorithm { 
            get { return _defaultLayoutAlgorithm; } set { _defaultLayoutAlgorithm = value; SetDefaultParams(0); } 
        }

        private OverlapRemovalAlgorithmTypeEnum _defaultOverlapRemovalAlgorithm;
        public OverlapRemovalAlgorithmTypeEnum DefaultOverlapRemovalAlgorithm {
            get { return _defaultOverlapRemovalAlgorithm; }
            set { _defaultOverlapRemovalAlgorithm = value; SetDefaultParams(1);}
        }

        private EdgeRoutingAlgorithmTypeEnum _defaultEdgeRoutingAlgorithm;
        public EdgeRoutingAlgorithmTypeEnum DefaultEdgeRoutingAlgorithm { 
            get { return _defaultEdgeRoutingAlgorithm; } set { _defaultEdgeRoutingAlgorithm = value; SetDefaultParams(2); } }

        public ILayoutParameters DefaultLayoutAlgorithmParams { get; set; }
        public IOverlapRemovalParameters DefaultOverlapRemovalAlgorithmParams { get; set; }
        public IEdgeRoutingParameters DefaultEdgeRoutingAlgorithmParams { get; set; }
        public bool AsyncAlgorithmCompute { get; set; }

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
        /// Use edge curving technique for smoother edges. Default value is false.
        /// </summary>
        public bool EdgeCurvingEnabled { get; set; }

        /// <summary>
        /// This is roughly the length of each line segment in the polyline
        /// approximation to a continuous curve in WPF units.  The smaller the
        /// number the smoother the curve, but slower the performance. Default is 8.
        /// </summary>
        public double EdgeCurvingTolerance { get; set; }

        /// <summary>
        /// Radius of a self-loop edge, which is drawn as a circle. Default is 20.
        /// </summary>
        public double EdgeSelfLoopCircleRadius { get; set; }

        /// <summary>
        /// Offset from the corner of the vertex. Useful for custom vertex shapes. Default is 10,10.
        /// </summary>
        public Point EdgeSelfLoopCircleOffset { get; set; }

        /// <summary>
        /// Show self looped edges on vertices. Default value is true.
        /// </summary>
        public bool EdgeShowSelfLooped { get; set; }

        /// <summary>
        /// Main graph object
        /// </summary>
        public virtual TGraph Graph { get; set; }

        /// <summary>
        /// Enables parallel edges. All edges between the same nodes will be separated by ParallelEdgeDistance value.
        /// This is post-process procedure and it may be performance-costly.
        /// </summary>
        public bool EnableParallelEdges { get; set; }

        /// <summary>
        /// Distance by which edges are parallelized if EnableParallelEdges is true. Default value is 5.
        /// </summary>
        public int ParallelEdgeDistance { get; set; }

        #region IsEdgeRoutingEnabled
        /// <summary>
        /// Value overloaded for extensibility purposes. Indicates if ER will be performed on Compute().
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
            EdgeSelfLoopCircleOffset = new Point(10, 10);
            EdgeCurvingTolerance = 8;
            EdgeSelfLoopCircleRadius = 10;
            EdgeShowSelfLooped = true;
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
            if(_vertexPosSource != null)
                _vertexPosSource.Clear();
        }


        public void CreateNewAlgorithmFactory()
        {
            AlgorithmFactory = new AlgorithmFactory<TVertex, TEdge, TGraph>();
        }

        public void CreateNewAlgorithmStorage(IExternalLayout<TVertex> layout, IExternalOverlapRemoval<TVertex> or, IExternalEdgeRouting<TVertex, TEdge> er)
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
            
            //setup overlap removal algorythm
            if (AreOverlapNeeded())
                algOverlap = GenerateOverlapRemovalAlgorithm();
            var algEr = GenerateEdgeRoutingAlgorithm(CalculateContentRectangle().Size);

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
        /// <param name="positions">Vertex positions collection (auto filled if null)</param>
        /// <param name="vertexSizes">Vertex sizes collection (auto filled if null)</param>
        /// <param name="getCenterPoints">True if you want center points returned instead of top-left (needed by overlap removal algo)</param>
        public Dictionary<TVertex, Rect> GetVertexSizeRectangles(IDictionary<TVertex, Point> positions = null, Dictionary<TVertex, Size> vertexSizes = null, bool getCenterPoints = false)
        {

            if (vertexSizes == null || positions == null)
                throw new GX_InvalidDataException("GetVertexSizeRectangles() -> Vertex sizes or positions not set!");
            var rectangles = new Dictionary<TVertex, Rect>();
            foreach (var vertex in Graph.Vertices.Where(a => a.SkipProcessing != ProcessingOptionEnum.Exclude))
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
