using System.Collections.Generic;
using GraphX.Controls;

namespace GraphX
{
    public interface IGraphArea<TVertex>
    {
        IDictionary<TVertex, VertexControl> VertexList { get; set; }
    }
}
