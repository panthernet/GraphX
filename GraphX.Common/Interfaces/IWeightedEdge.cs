using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphX
{
    public interface IWeightedEdge<TVertex> : IEdge<TVertex>
    {
        double Weight { get; set; }
    }
}
