namespace GraphX
{
    /// <summary>
    /// Core vertex data interface
    /// </summary>
    /// <typeparam name="TVertex">Vertex data type</typeparam>
    public interface IGraphXEdge<TVertex> : IWeightedEdge<TVertex>, IIdentifiableGraphDataObject, IRoutingInfo
    {
        /// <summary>
        /// Source vertex
        /// </summary>
        new TVertex Source { get; set; }
        /// <summary>
        /// Target vertex
        /// </summary>
        new TVertex Target { get; set; }
        /// <summary>
        /// If edge is self-looped
        /// </summary>
        bool IsSelfLoop { get; }
    }
}