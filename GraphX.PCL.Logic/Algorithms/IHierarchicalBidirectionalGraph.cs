using System.Collections.Generic;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms
{
	public interface IHierarchicalBidirectionalGraph<TVertex, TEdge> : IBidirectionalGraph<TVertex, TEdge>
		where TEdge : TypedEdge<TVertex>
	{
		IEnumerable<TEdge> HierarchicalEdgesFor(TVertex v);
		int HierarchicalEdgeCountFor(TVertex v);
		IEnumerable<TEdge> HierarchicalEdges { get; }
		int HierarchicalEdgeCount { get; }

		IEnumerable<TEdge> InHierarchicalEdges(TVertex v);
		int InHierarchicalEdgeCount(TVertex v);

		IEnumerable<TEdge> OutHierarchicalEdges(TVertex v);
		int OutHierarchicalEdgeCount(TVertex v);

		IEnumerable<TEdge> GeneralEdgesFor(TVertex v);
		int GeneralEdgeCountFor(TVertex v);
		IEnumerable<TEdge> GeneralEdges { get; }
		int GeneralEdgeCount { get; }

		IEnumerable<TEdge> InGeneralEdges(TVertex v);
		int InGeneralEdgeCount(TVertex v);

		IEnumerable<TEdge> OutGeneralEdges(TVertex v);
		int OutGeneralEdgeCount(TVertex v);
	}
}