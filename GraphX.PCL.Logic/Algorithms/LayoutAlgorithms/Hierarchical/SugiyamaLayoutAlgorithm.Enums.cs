using System;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public partial class SugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph> 
        where TVertex : class 
        where TEdge : IEdge<TVertex> 
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
		[Flags]
		protected enum BaryCenter
		{
			Up = 1,
			Down = 2,
			Sub = 4
		}

		[Flags]
		protected enum CrossCount
		{
			Up = 1,
			Down = 2
		}

		protected enum SweepingDirection
		{
			Up = 1,
			Down = 2
		}
	}
}