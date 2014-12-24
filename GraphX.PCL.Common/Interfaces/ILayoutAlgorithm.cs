using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout
{
    public interface ILayoutAlgorithm<TVertex, TEdge, TGraph> : IExternalLayout<TVertex>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
        TGraph VisitedGraph { get; }
       ////// Dictionary<TVertex, Point> FreezedVertices { get; } 
	}
}