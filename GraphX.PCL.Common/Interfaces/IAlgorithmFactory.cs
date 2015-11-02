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
        /// <summary>
        /// Returns True if specified layout algorithm needs vertex size data for its calculations
        /// </summary>
        /// <param name="algorithmType">Layout algorithm type</param>
        bool NeedSizes(LayoutAlgorithmTypeEnum algorithmType);
        /// <summary>
        /// Returns True if specified layout algorithm ever needs edge routing pass
        /// </summary>
        /// <param name="algorithmType">Layout algorithm type</param>
        bool NeedEdgeRouting(LayoutAlgorithmTypeEnum algorithmType);
        /// <summary>
        /// Returns True if specified layout algorithm ever needs overlap removal pass
        /// </summary>
        /// <param name="algorithmType">Layout algorithm type</param>
        bool NeedOverlapRemoval(LayoutAlgorithmTypeEnum algorithmType);
        /// <summary>
        /// Creates parameters data for layout algorithm
        /// </summary>
        /// <param name="algorithmType">Layout algorithm type</param>
        ILayoutParameters CreateLayoutParameters(LayoutAlgorithmTypeEnum algorithmType);
        /// <summary>
        /// Generate and initialize layout algorithm
        /// </summary>
        /// <param name="newAlgorithmType">Layout algorithm type</param>
        /// <param name="iGraph">Graph</param>
        /// <param name="positions">Optional vertex positions</param>
        /// <param name="sizes">Optional vertex sizes</param>
        /// <param name="parameters">Optional algorithm parameters</param>
        ILayoutAlgorithm<TVertex, TEdge, TGraph> CreateLayoutAlgorithm(LayoutAlgorithmTypeEnum newAlgorithmType, TGraph iGraph, IDictionary<TVertex, Point> positions = null, IDictionary<TVertex, Size> sizes = null, ILayoutParameters parameters = null);
        /// <summary>
        /// Create and initializes overlap removal algorithm
        /// </summary>
        /// <param name="newAlgorithmType">Algorithm type</param>
        /// <param name="rectangles">Object sizes list</param>
        /// <param name="parameters">Optional algorithm parameters</param>
        IOverlapRemovalAlgorithm<TVertex> CreateOverlapRemovalAlgorithm(OverlapRemovalAlgorithmTypeEnum newAlgorithmType, IDictionary<TVertex, Rect> rectangles, IOverlapRemovalParameters parameters = null);
        /// <summary>
        /// Create uninitialized overlap removal algorithm
        /// </summary>
        /// <param name="newAlgorithmType">Algorithm type</param>
        IOverlapRemovalAlgorithm<TVertex> CreateOverlapRemovalAlgorithm(OverlapRemovalAlgorithmTypeEnum newAlgorithmType);
        /// <summary>
        /// Create overlap removal algorithm parameters
        /// </summary>
        /// <param name="algorithmType">Overlap removal algorithm type</param>
        IOverlapRemovalParameters CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum algorithmType);
        IExternalEdgeRouting<TVertex, TEdge> CreateEdgeRoutingAlgorithm(EdgeRoutingAlgorithmTypeEnum newAlgorithmType, Rect graphArea, TGraph iGraph, IDictionary<TVertex, Point> positions, IDictionary<TVertex, Rect> rectangles, IEdgeRoutingParameters parameters = null);
        IEdgeRoutingParameters CreateEdgeRoutingParameters(EdgeRoutingAlgorithmTypeEnum algorithmType);

        /// <summary>
        /// Create and initialize FSAA overlap removal algorithm instance
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="rectangles">Size data</param>
        /// <param name="horGap">Horizontal gap setting</param>
        /// <param name="vertGap">Vertical gap setting</param>
        IOverlapRemovalAlgorithm<T> CreateFSAA<T>(IDictionary<T, Rect> rectangles, float horGap, float vertGap) where T : class;
        /// <summary>
        /// Create uninitialized FSAA overlap removal algorithm instance
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="horGap">Horizontal gap setting</param>
        /// <param name="vertGap">Vertical gap setting</param>
        IOverlapRemovalAlgorithm<T, IOverlapRemovalParameters> CreateFSAA<T>(float horGap, float vertGap) where T : class;
    }
}
