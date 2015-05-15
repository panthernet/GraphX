using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public interface ILayoutContext<TVertex, TEdge, out TGraph>
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
    {
        IDictionary<TVertex, Point> Positions { get; }
        IDictionary<TVertex, Size> Sizes { get; }

        TGraph Graph { get; }

        LayoutMode Mode { get; }
    }
}