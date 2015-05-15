using QuickGraph;

namespace GraphX.PCL.Common.Interfaces
{
    public interface ILayoutAlgorithm<TVertex, TEdge, out TGraph> : IExternalLayout<TVertex>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
        TGraph VisitedGraph { get; }
       ////// Dictionary<TVertex, Point> FreezedVertices { get; } 
	}
}