using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GraphX.GraphSharp.Algorithms.Layout;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphX.Logic.Models
{
    public sealed class AlgorithmStorage<TVertex, TEdge> : IAlgorithmStorage<TVertex, TEdge>
    {
        public IExternalLayout<TVertex> Layout { get; private set; }
        public IExternalOverlapRemoval<TVertex> OverlapRemoval { get; private set; }
        public IExternalEdgeRouting<TVertex, TEdge> EdgeRouting { get; private set; }

        public AlgorithmStorage(IExternalLayout<TVertex> layout, IExternalOverlapRemoval<TVertex> or, IExternalEdgeRouting<TVertex, TEdge> er)
        {
            Layout = layout; OverlapRemoval = or; EdgeRouting = er;
        }
    }
}
