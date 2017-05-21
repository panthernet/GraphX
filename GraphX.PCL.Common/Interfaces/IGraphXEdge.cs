namespace GraphX.PCL.Common.Interfaces
{
    /// <summary>
    /// Core GraphX edge data interface
    /// </summary>
    /// <typeparam name="TVertex">Vertex data type</typeparam>
    public interface IGraphXEdge<TVertex> : IGraphXCommonEdge, IWeightedEdge<TVertex>
    {
        /// <summary>
        /// Gets or sets source vertex
        /// </summary>
        new TVertex Source { get; set; }

        /// <summary>
        /// Gets or sets target vertex
        /// </summary>
        new TVertex Target { get; set; }
    }

    /// <summary>
    /// Core GraphX edge data interface
    /// </summary>
    public interface IGraphXEdge : IGraphXCommonEdge, IWeightedEdge<IGraphXVertex>
    {
        /// <summary>
        /// Gets or sets source vertex
        /// </summary>
        new IGraphXVertex Source { get; set; }

        /// <summary>
        /// Gets or sets target vertex
        /// </summary>
        new IGraphXVertex Target { get; set; }
    }

    /// <summary>
    /// Core edge data interface
    /// </summary>
    public interface IGraphXCommonEdge : IIdentifiableGraphDataObject, IRoutingInfo
    {
        /// <summary>
        /// Gets if edge is self-looped
        /// </summary>
        bool IsSelfLoop { get; }

        /// <summary>
        /// Optional parameter to bind edge to static vertex connection point
        /// </summary>
        int? SourceConnectionPointId { get; set; }

        /// <summary>
        /// Optional parameter to bind edge to static vertex connection point
        /// </summary>
        int? TargetConnectionPointId { get; set; }

        /// <summary>
        /// Reverse the calculated routing path points.
        /// </summary>
        bool ReversePath { get; set; }
    }
}