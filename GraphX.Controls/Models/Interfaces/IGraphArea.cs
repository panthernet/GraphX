using System.Collections.Generic;

namespace GraphX.Controls.Models.Interfaces
{
    public interface IGraphArea<TVertex>
    {
        IDictionary<TVertex, VertexControl> VertexList { get; set; }
    }
}
