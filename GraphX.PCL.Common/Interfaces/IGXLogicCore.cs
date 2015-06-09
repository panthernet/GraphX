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
        IAlgorithmFactory<TVertex, TEdge, TGraph> AlgorithmFactory { get; }
        IAlgorithmStorage<TVertex, TEdge> AlgorithmStorage { get; }
        TGraph Graph { get; set; }
        bool AsyncAlgorithmCompute { get; set; }

        bool EdgeCurvingEnabled { get; set; }
        double EdgeCurvingTolerance { get; set; }
        /// <summary>
        /// Gets or sets looped edge default indicator (path circle) radius
        /// </summary>
        double EdgeSelfLoopElementRadius { get; set; }
        /// <summary>
        /// Gets or sets looped edge offset form top-left vertex corner
        /// </summary>
        Point EdgeSelfLoopElementOffset { get; set; }
        /// <summary>
        /// Gets or sets if self looped edge indicators are visible
        /// </summary>
        bool EdgeShowSelfLooped { get; set; }
        bool EnableParallelEdges { get; set; }
        int ParallelEdgeDistance { get; set; }
        bool IsEdgeRoutingEnabled { get; }
        bool EnableEdgeLabelsOverlapRemoval { get; set; }
        bool IsCustomLayout { get; }

        LayoutAlgorithmTypeEnum DefaultLayoutAlgorithm { get; set; }
        OverlapRemovalAlgorithmTypeEnum DefaultOverlapRemovalAlgorithm { get; set; }
        EdgeRoutingAlgorithmTypeEnum DefaultEdgeRoutingAlgorithm { get; set; }
        ILayoutParameters DefaultLayoutAlgorithmParams { get; set; }
        IOverlapRemovalParameters DefaultOverlapRemovalAlgorithmParams { get; set; }
        IEdgeRoutingParameters DefaultEdgeRoutingAlgorithmParams { get; set; }

        IExternalLayout<TVertex> ExternalLayoutAlgorithm { get; set; }
        IExternalOverlapRemoval<TVertex> ExternalOverlapRemovalAlgorithm { get; set; }
        IExternalEdgeRouting<TVertex, TEdge> ExternalEdgeRoutingAlgorithm { get; set; }

        void ComputeEdgeRoutesByVertex(TVertex dataVertex, Point? vertexPosition = null, Size? vertexSize = null);
        void CreateNewAlgorithmFactory();
        void CreateNewAlgorithmStorage(IExternalLayout<TVertex> layout, IExternalOverlapRemoval<TVertex> or, IExternalEdgeRouting<TVertex, TEdge> er);

        IDictionary<TVertex, Point> Compute(CancellationToken cancellationToken);

        bool AreVertexSizesNeeded();
        bool AreOverlapNeeded();
        IExternalLayout<TVertex> GenerateLayoutAlgorithm(Dictionary<TVertex, Size> vertexSizes, IDictionary<TVertex, Point> vertexPositions);
        IExternalOverlapRemoval<TVertex> GenerateOverlapRemovalAlgorithm(Dictionary<TVertex, Rect> rectangles = null);
        IExternalEdgeRouting<TVertex, TEdge> GenerateEdgeRoutingAlgorithm(Size desiredSize, IDictionary<TVertex, Point> positions = null, IDictionary<TVertex, Rect> rectangles = null);

        /// <summary>
        /// Creates algorithms by values in LogicCore properties and generates new AlgorithmStorage object
        /// </summary>
        /// <param name="vertexSizes">Vertex sizes</param>
        /// <param name="vertexPositions">Vertex positions</param>
        bool GenerateAlgorithmStorage(Dictionary<TVertex, Size> vertexSizes,
            IDictionary<TVertex, Point> vertexPositions);
    }
}
