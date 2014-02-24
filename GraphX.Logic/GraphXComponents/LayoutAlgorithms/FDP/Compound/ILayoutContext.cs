using System.Collections.Generic;
using System.Diagnostics.Contracts;
using QuickGraph;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.Layout
{
    public interface ILayoutContext<TVertex, TEdge, TGraph>
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
    {
        IDictionary<TVertex, Point> Positions { get; }
        IDictionary<TVertex, Size> Sizes { get; }

        TGraph Graph { get; }

        LayoutMode Mode { get; }
    }
}