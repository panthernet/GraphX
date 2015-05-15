using QuickGraph;

namespace GraphX.PCL.Common.Interfaces
{
    public interface IWeightedEdge<TVertex> : IEdge<TVertex>
    {
        double Weight { get; set; }
    }
}