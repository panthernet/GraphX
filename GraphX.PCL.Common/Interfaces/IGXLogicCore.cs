using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GraphX.GraphSharp.Algorithms.Layout;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using GraphX.Measure;
using GraphX.Models;
using QuickGraph;
using System;
using System.Collections.Generic;

namespace GraphX
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
        double EdgeSelfLoopCircleRadius { get; set; }
        Point EdgeSelfLoopCircleOffset { get; set; }
        bool EdgeShowSelfLooped { get; set; }
        bool EnableParallelEdges { get; set; }
        int ParallelEdgeDistance { get; set; }
        bool IsEdgeRoutingEnabled { get; }
        bool EnableEdgeLabelsOverlapRemoval { get; set; }
        bool IsCustomLayout { get; }

        IFileServiceProvider FileServiceProvider { get; set; }

        LayoutAlgorithmTypeEnum DefaultLayoutAlgorithm { get; set; }
        OverlapRemovalAlgorithmTypeEnum DefaultOverlapRemovalAlgorithm { get; set; }
        EdgeRoutingAlgorithmTypeEnum DefaultEdgeRoutingAlgorithm { get; set; }
        ILayoutParameters DefaultLayoutAlgorithmParams { get; set; }
        IOverlapRemovalParameters DefaultOverlapRemovalAlgorithmParams { get; set; }
        IEdgeRoutingParameters DefaultEdgeRoutingAlgorithmParams { get; set; }


        void CreateNewAlgorithmFactory();
        void CreateNewAlgorithmStorage(IExternalLayout<TVertex> layout, IExternalOverlapRemoval<TVertex> or, IExternalEdgeRouting<TVertex, TEdge> er);

        bool AreVertexSizesNeeded();
        bool AreOverlapNeeded();
        IExternalLayout<TVertex> GenerateLayoutAlgorithm(Dictionary<TVertex, Size> vertexSizes);
        IExternalOverlapRemoval<TVertex> GenerateOverlapRemovalAlgorithm(Dictionary<TVertex, Rect> rectangles = null);
        IExternalEdgeRouting<TVertex, TEdge> GenerateEdgeRoutingAlgorithm(Size DesiredSize);

    }
}
