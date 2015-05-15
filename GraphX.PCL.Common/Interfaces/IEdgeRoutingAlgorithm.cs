using QuickGraph;

namespace GraphX.PCL.Common.Interfaces
{
    /// <summary>
    /// obsolete?
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    public interface IEdgeRoutingAlgorithm<TVertex, TEdge> : IExternalEdgeRouting<TVertex, TEdge>
		where TEdge : IEdge<TVertex>
	{

	}
}