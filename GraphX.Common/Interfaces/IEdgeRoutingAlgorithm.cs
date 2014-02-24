using System.Collections.Generic;
using QuickGraph.Algorithms;
using QuickGraph;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.EdgeRouting
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