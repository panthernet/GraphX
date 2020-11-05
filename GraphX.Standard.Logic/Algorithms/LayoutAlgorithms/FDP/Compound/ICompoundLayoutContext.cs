using System.Collections.Generic;
using GraphX.Measure;
using QuikGraph;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public interface ICompoundLayoutContext<TVertex, TEdge, out TGraph> : ILayoutContext<TVertex, TEdge, TGraph>
        where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        IDictionary<TVertex, Thickness> VertexBorders { get; }
        IDictionary<TVertex, CompoundVertexInnerLayoutType> LayoutTypes { get; }
    }
}
