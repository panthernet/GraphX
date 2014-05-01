using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphX
{
    public interface IGraphXEdge<TVertex> : IWeightedEdge<TVertex>, IIdentifiableGraphDataObject, IRoutingInfo
    {
        new TVertex Source { get; set; }
        new TVertex Target { get; set; }
        bool IsSelfLoop { get; }
    }
}
