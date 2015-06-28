using System;
using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using QuickGraph;

namespace GraphX.PCL.Common.Interfaces
{
    public interface IGXLogicCore<TVertex, TEdge, TGraph>: IDisposable
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
    {
        /// <summary>
        /// Get an algorithm factory that provides different algorithm creation methods
        /// </summary>
        IAlgorithmFactory<TVertex, TEdge, TGraph> AlgorithmFactory { get; }
        /// <summary>
        /// Gets or sets algorithm storage that contain all currently defined algorithm objects by type (default or external)
        /// Actual storage data is vital for correct edge routing operation after graph was regenerated.
        /// </summary>
        IAlgorithmStorage<TVertex, TEdge> AlgorithmStorage { get; set; }

        /// <summary>
        /// Gets or sets main graph object
        /// </summary>
        TGraph Graph { get; set; }

        /// <summary>
        /// Gets or sets if async algorithm computations are enabled
        /// </summary>
        bool AsyncAlgorithmCompute { get; set; }

        /// <summary>
        /// Gets or sets if edge curving technique enabled for smoother edges. Default value is True.
        /// </summary>
        bool EdgeCurvingEnabled { get; set; }

        /// <summary>
        /// Gets or sets roughly the length of each line segment in the polyline
        /// approximation to a continuous curve in WPF units.  The smaller the
        /// number the smoother the curve, but slower the performance. Default is 8.
        /// </summary>
        double EdgeCurvingTolerance { get; set; }

        /// <summary>
        /// Gets or sets if parallel edges are enabled. All edges between the same nodes will be separated by ParallelEdgeDistance value.
        /// This is post-process procedure and it may be performance-costly.
        /// </summary>
        bool EnableParallelEdges { get; set; }

        /// <summary>
        /// Gets or sets distance by which edges are parallelized if EnableParallelEdges is true. Default value is 5.
        /// </summary>
        int ParallelEdgeDistance { get; set; }

        /// <summary>
        /// Gets if edge routing will be performed on Compute() method execution
        /// </summary>
        bool IsEdgeRoutingEnabled { get; }

        //bool EnableEdgeLabelsOverlapRemoval { get; set; }

        /// <summary>
        /// Gets is LayoutAlgorithmTypeEnum.Custom (NOT external) layout selected and used. Custom layout used to manualy generate graph.
        /// </summary>
        bool IsCustomLayout { get; }

        /// <summary>
        /// Gets or sets default layout algorithm that will be used on graph generation/relayouting
        /// </summary>
        LayoutAlgorithmTypeEnum DefaultLayoutAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets default overlap removal algorithm that will be used on graph generation/relayouting
        /// </summary>
        OverlapRemovalAlgorithmTypeEnum DefaultOverlapRemovalAlgorithm { get; set; }
        
        /// <summary>
        /// Gets or sets default edge routing algorithm that will be used on graph generation/relayouting
        /// </summary>
        EdgeRoutingAlgorithmTypeEnum DefaultEdgeRoutingAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets default layout algorithm parameters that will be used on graph generation/relayouting
        /// </summary>
        ILayoutParameters DefaultLayoutAlgorithmParams { get; set; }
       
        /// <summary>
        /// Gets or sets default overlap removal algorithm parameters that will be used on graph generation/relayouting
        /// </summary>
        IOverlapRemovalParameters DefaultOverlapRemovalAlgorithmParams { get; set; }

        /// <summary>
        /// Gets or sets default edge routing algorithm parameters that will be used on graph generation/relayouting
        /// </summary>
        IEdgeRoutingParameters DefaultEdgeRoutingAlgorithmParams { get; set; }

        /// <summary>
        /// Gets or sets external layout algorithm that will be used instead of the default one.
        /// Negates DefaultLayoutAlgorithm property value if set.
        /// </summary>
        IExternalLayout<TVertex> ExternalLayoutAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets external overlap removal algorithm that will be used instead of the default one.
        /// Negates DefaultOverlapRemovalAlgorithm property value if set.
        /// </summary>
        IExternalOverlapRemoval<TVertex> ExternalOverlapRemovalAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets external edge routing algorithm that will be used instead of the default one.
        /// Negates DefaultEdgeRoutingAlgorithm property value if set.
        /// </summary>
        IExternalEdgeRouting<TVertex, TEdge> ExternalEdgeRoutingAlgorithm { get; set; }

        /// <summary>
        /// Computes all edge routes related to specified vertex
        /// </summary>
        /// <param name="dataVertex">Vertex data</param>
        /// <param name="vertexPosition">Vertex position</param>
        /// <param name="vertexSize">Vertex size</param>
        void ComputeEdgeRoutesByVertex(TVertex dataVertex, Point? vertexPosition = null, Size? vertexSize = null);

        //void CreateNewAlgorithmFactory();
        //void CreateNewAlgorithmStorage(IExternalLayout<TVertex> layout, IExternalOverlapRemoval<TVertex> or, IExternalEdgeRouting<TVertex, TEdge> er);

        /// <summary>
        /// Computes graph using parameters set in LogicCore properties
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token for async operation abort</param>
        IDictionary<TVertex, Point> Compute(CancellationToken cancellationToken);

        /// <summary>
        /// Gets if current algorithm set needs vertices sizes
        /// </summary>
        bool AreVertexSizesNeeded();

        /// <summary>
        /// Gets if current algorithms set actualy needs overlap removal algorithm
        /// </summary>
        bool AreOverlapNeeded();

        /// <summary>
        /// Generate layout algorithm according to LogicCore layout algorithm default/external properties set
        /// </summary>
        /// <param name="vertexSizes">Vertices sizes</param>
        /// <param name="vertexPositions">Vertices positions</param>
        IExternalLayout<TVertex> GenerateLayoutAlgorithm(Dictionary<TVertex, Size> vertexSizes, IDictionary<TVertex, Point> vertexPositions);

        /// <summary>
        /// Generate overlap removal algorithm according to LogicCore overlap removal algorithm default/external properties set
        /// </summary>
        /// <param name="rectangles">Vertices rectangular sizes</param>
        IExternalOverlapRemoval<TVertex> GenerateOverlapRemovalAlgorithm(Dictionary<TVertex, Rect> rectangles = null);

        /// <summary>
        /// Generate layout algorithm according to LogicCore layout algorithm default/external properties set
        /// </summary>
        /// <param name="desiredSize">Desired rectangular area size that will be taken into account</param>
        /// <param name="vertexPositions">Vertices positions</param>
        /// <param name="rectangles">Vertices rectangular sizes</param>
        IExternalEdgeRouting<TVertex, TEdge> GenerateEdgeRoutingAlgorithm(Size desiredSize, IDictionary<TVertex, Point> vertexPositions = null, IDictionary<TVertex, Rect> rectangles = null);

        /// <summary>
        /// Creates algorithm objects based on default/external LogicCore properies and stores them to be able to access them later by, for ex. edge recalculation logic.
        /// Done automaticaly when graph is regenerated/relayouted.
        /// </summary>
        /// <param name="vertexSizes">Vertex sizes</param>
        /// <param name="vertexPositions">Vertex positions</param>
        bool GenerateAlgorithmStorage(Dictionary<TVertex, Size> vertexSizes,
            IDictionary<TVertex, Point> vertexPositions);
    }
}
