using System.Collections.Generic;
using GraphX.Controls;

namespace GraphX.METRO.Controls.Models.Interfaces
{
    public interface IGraphArea<TVertex>
    {
        IDictionary<TVertex, VertexControl> VertexList { get; set; }
    }
}
