using System.Collections.Generic;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using QuickGraph;

namespace GraphX.PCL.Common.Interfaces
{
    public interface IAlgorithmFactory<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
    {
        bool NeedSizes(LayoutAlgorithmTypeEnum algorithmType);
        bool NeedEdgeRouting(LayoutAlgorithmTypeEnum algorithmType);
        bool NeedOverlapRemoval(LayoutAlgorithmTypeEnum algorithmType);
        ILayoutParameters CreateLayoutParameters(LayoutAlgorithmTypeEnum algorithmType);
        ILayoutAlgorithm<TVertex, TEdge, TGraph> CreateLayoutAlgorithm(LayoutAlgorithmTypeEnum newAlgorithmType, TGraph iGraph, IDictionary<TVertex, Point> positions = null, IDictionary<TVertex, Size> sizes = null, ILayoutParameters parameters = null);
        IOverlapRemovalAlgorithm<TVertex> CreateOverlapRemovalAlgorithm(OverlapRemovalAlgorithmTypeEnum newAlgorithmType, IDictionary<TVertex, Rect> rectangles, IOverlapRemovalParameters parameters = null);
        IOverlapRemovalParameters CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum algorithmType);
        IExternalEdgeRouting<TVertex, TEdge> CreateEdgeRoutingAlgorithm(EdgeRoutingAlgorithmTypeEnum newAlgorithmType, Rect graphArea, TGraph iGraph, IDictionary<TVertex, Point> positions, IDictionary<TVertex, Rect> rectangles, IEdgeRoutingParameters parameters = null);
        IEdgeRoutingParameters CreateEdgeRoutingParameters(EdgeRoutingAlgorithmTypeEnum algorithmType);
        IOverlapRemovalAlgorithm<T> CreateFSAA<T>(IDictionary<T, Rect> rectangles, float horgap, float vertGap) where T : class;
    }
}
