using QuickGraph;

namespace GraphX.PCL.Common.Interfaces
{
    /// <summary>
    /// Common interface for the graph filter
    /// </summary>
    /// <typeparam name="TVertex">Graph vertex</typeparam>
    /// <typeparam name="TEdge">Graph edge</typeparam>
    /// <typeparam name="TGraph">Graph</typeparam>
    public interface IGraphFilter<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
    {
        /// <summary>
        /// Core filter method that can modify the input graph and supply modified graph to the output
        /// </summary>
        /// <param name="inputGraph">Input graph</param>
        TGraph ProcessFilter(TGraph inputGraph);
    }
}
