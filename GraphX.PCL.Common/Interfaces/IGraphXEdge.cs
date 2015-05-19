namespace GraphX.PCL.Common.Interfaces
{
    /// <summary>
    /// Core GraphX edge data interface
    /// </summary>
    /// <typeparam name="TVertex">Vertex data type</typeparam>
    public interface IGraphXEdge<TVertex> : IGraphXCommonEdge, IWeightedEdge<TVertex>
    {
        /// <summary>
        /// Source vertex
        /// </summary>
        new TVertex Source { get; set; }
        /// <summary>
        /// Target vertex
        /// </summary>
        new TVertex Target { get; set; }

    }

    /// <summary>
    /// Core edge data interface
    /// </summary>
    public interface IGraphXCommonEdge: IIdentifiableGraphDataObject, IRoutingInfo
    {
        /// <summary>
        /// If edge is self-looped
        /// </summary>
        bool IsSelfLoop { get; }
        /// <summary>
        /// Optional parameter to bind edge to static vertex connection point
        /// </summary>
        int? SourceConnectionPointId { get; }
        /// <summary>
        /// Optional parameter to bind edge to static vertex connection point
        /// </summary>
        int? TargetConnectionPointId { get; }
    }
}