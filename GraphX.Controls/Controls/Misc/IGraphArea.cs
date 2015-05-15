using System.Collections.Generic;

namespace GraphX.WPF.Controls
{
    public interface IGraphArea<TVertex>
    {
        IDictionary<TVertex, VertexControl> VertexList { get; set; }
    }
}
