using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GraphX.GraphSharp.Algorithms.Layout;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GraphX
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
        ILayoutAlgorithm<TVertex, TEdge, TGraph> CreateLayoutAlgorithm(LayoutAlgorithmTypeEnum newAlgorithmType, TGraph Graph, IDictionary<TVertex, Point> Positions = null, IDictionary<TVertex, Size> Sizes = null, ILayoutParameters parameters = null);
        IOverlapRemovalAlgorithm<TVertex> CreateOverlapRemovalAlgorithm(OverlapRemovalAlgorithmTypeEnum newAlgorithmType, IDictionary<TVertex, Rect> Rectangles, IOverlapRemovalParameters parameters = null);
        IOverlapRemovalParameters CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum algorithmType);
        IExternalEdgeRouting<TVertex, TEdge> CreateEdgeRoutingAlgorithm(EdgeRoutingAlgorithmTypeEnum newAlgorithmType, Rect graphArea, TGraph Graph, IDictionary<TVertex, Point> Positions, IDictionary<TVertex, Rect> Rectangles, IEdgeRoutingParameters parameters = null);
        IEdgeRoutingParameters CreateEdgeRoutingParameters(EdgeRoutingAlgorithmTypeEnum algorithmType);
        IOverlapRemovalAlgorithm<T> CreateFSAA<T>(IDictionary<T, Rect> rectangles, float horgap, float vertGap) where T : class;
    }
}
