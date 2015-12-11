using GraphX.Measure;

namespace GraphX.PCL.Common.Interfaces
{
    /// <summary>
    /// Base level interface for algorithm storage implementation
    /// </summary>
    /// <typeparam name="TVertex">Vertex data class</typeparam>
    /// <typeparam name="TEdge">Edge data class</typeparam>
    public interface IAlgorithmStorage<TVertex, TEdge>
    {
        /// <summary>
        /// Gets layout algorithm
        /// </summary>
        IExternalLayout<TVertex, TEdge> Layout { get; }

        /// <summary>
        /// Gets overlap removal algorithm
        /// </summary>
        IExternalOverlapRemoval<TVertex> OverlapRemoval { get; }

        /// <summary>
        /// Gets edge routing algorithm
        /// </summary>
        IExternalEdgeRouting<TVertex, TEdge> EdgeRouting { get; }

        void RemoveSingleEdge(TEdge edge);
        void AddSingleEdge(TEdge edge, Point[] routingPoints = null);
        void RemoveSingleVertex(TVertex vertex);
        void AddSingleVertex(TVertex vertex, Point position, Rect size);
    }
}
