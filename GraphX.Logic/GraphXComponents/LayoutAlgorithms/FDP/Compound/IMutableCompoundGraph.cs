using QuickGraph;

namespace GraphX.GraphSharp
{
    public interface IMutableCompoundGraph<TVertex, TEdge> 
        : ICompoundGraph<TVertex, TEdge>,
          IMutableBidirectionalGraph<TVertex, TEdge> 
        where TEdge : IEdge<TVertex>
    {
    }
}
