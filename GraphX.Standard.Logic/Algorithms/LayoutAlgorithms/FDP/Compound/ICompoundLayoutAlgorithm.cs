using System.Collections.Generic;
using GraphX.Measure;
using GraphX.Common.Interfaces;
using QuickGraph;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
	public interface ICompoundLayoutAlgorithm<TVertex, TEdge, out TGraph> : ILayoutAlgorithm<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
	    IDictionary<TVertex, Size> InnerCanvasSizes { get; }
	}
}
