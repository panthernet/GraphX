using System.Collections.Generic;
using System.Windows;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout.Compound
{
	public interface ICompoundLayoutAlgorithm<TVertex, TEdge, TGraph> : ILayoutAlgorithm<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
	    IDictionary<TVertex, Size> InnerCanvasSizes { get; }
	}
}
