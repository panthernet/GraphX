using QuickGraph;

namespace GraphX
{
    public interface IWeightedEdge<TVertex> : IEdge<TVertex>
    {
        double Weight { get; set; }
    }
}