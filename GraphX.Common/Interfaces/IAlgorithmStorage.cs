using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GraphX.GraphSharp.Algorithms.Layout;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphX
{
    public interface IAlgorithmStorage<TVertex, TEdge>
    {
        IExternalLayout<TVertex> Layout { get; }
        IExternalOverlapRemoval<TVertex> OverlapRemoval { get; }
        IExternalEdgeRouting<TVertex, TEdge> EdgeRouting { get; }
    }
}
