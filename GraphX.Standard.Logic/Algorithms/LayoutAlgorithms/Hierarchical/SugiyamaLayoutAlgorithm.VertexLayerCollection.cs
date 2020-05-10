using System.Collections.Generic;
using QuikGraph;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
	public partial class SugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph> 
        where TVertex : class 
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
	{
		/// <summary>
		/// Collection of the layers of the vertices.
		/// </summary>
		private class VertexLayerCollection : List<VertexLayer>
		{
		}
	}
}