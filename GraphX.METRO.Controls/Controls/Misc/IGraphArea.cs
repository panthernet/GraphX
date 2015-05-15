using System.Collections.Generic;

namespace GraphX.METRO.Controls
{
    public interface IGraphArea<TVertex>
    {
        IDictionary<TVertex, VertexControl> VertexList { get; set; }
    }
}
