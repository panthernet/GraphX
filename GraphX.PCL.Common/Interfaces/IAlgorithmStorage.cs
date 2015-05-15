namespace GraphX.PCL.Common.Interfaces
{
    public interface IAlgorithmStorage<TVertex, TEdge>
    {
        IExternalLayout<TVertex> Layout { get; }
        IExternalOverlapRemoval<TVertex> OverlapRemoval { get; }
        IExternalEdgeRouting<TVertex, TEdge> EdgeRouting { get; }
    }
}
