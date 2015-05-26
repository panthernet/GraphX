using System.Collections.Generic;

namespace GraphX.Controls
{
    public interface IGraphArea<TVertex>
    {
        IDictionary<TVertex, VertexControl> VertexList { get; set; }
    }
}
