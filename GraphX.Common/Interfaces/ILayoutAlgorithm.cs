using System.Collections.Generic;
using QuickGraph.Algorithms;
using QuickGraph;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.Layout
{
    public interface ILayoutAlgorithm<TVertex, TEdge, TGraph> : IExternalLayout<TVertex>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
        TGraph VisitedGraph { get; }
	}
}