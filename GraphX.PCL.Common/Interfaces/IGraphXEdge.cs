namespace GraphX
{
    public interface IGraphXEdge<TVertex> : IWeightedEdge<TVertex>, IIdentifiableGraphDataObject, IRoutingInfo
    {
        new TVertex Source { get; set; }
        new TVertex Target { get; set; }
        bool IsSelfLoop { get; }
    }
}