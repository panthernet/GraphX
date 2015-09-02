using QuickGraph;

namespace GraphX.PCL.Common.Interfaces
{
    public interface ILayoutAlgorithm<TVertex, in TEdge, out TGraph> : IExternalLayout<TVertex, TEdge>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
        /// <summary>
        /// Internal graph storage for layout algorithm
        /// </summary>
        TGraph VisitedGraph { get; }
	}
}