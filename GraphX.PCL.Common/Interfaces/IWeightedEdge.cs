using QuickGraph;

namespace GraphX.PCL.Common.Interfaces
{
    public interface IWeightedEdge<TVertex> : IEdge<TVertex>
    {
        /// <summary>
        /// Edge weight that can be used by some weight-related layout algorithms
        /// </summary>
        double Weight { get; set; }
    }
}