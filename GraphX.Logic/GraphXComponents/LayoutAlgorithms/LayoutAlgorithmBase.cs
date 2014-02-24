using System.Collections.Generic;
using QuickGraph;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.Layout
{
	
	public abstract class LayoutAlgorithmBase<TVertex, TEdge, TGraph> : AlgorithmBase, ILayoutAlgorithm<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
		private readonly Dictionary<TVertex, Point> vertexPositions;
		private readonly TGraph visitedGraph;

		public IDictionary<TVertex, Point> VertexPositions
		{
			get { return vertexPositions; }
		}

		public TGraph VisitedGraph
		{
			get { return visitedGraph; }
		}

		protected LayoutAlgorithmBase( TGraph visitedGraph ) :
			this( visitedGraph, null )
		{
		}

		protected LayoutAlgorithmBase( TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions )
		{
            this.visitedGraph = visitedGraph;
			if ( vertexPositions != null )
				this.vertexPositions = new Dictionary<TVertex, Point>( vertexPositions );
			else
				this.vertexPositions = new Dictionary<TVertex, Point>( visitedGraph.VertexCount );
		}

        public IDictionary<TVertex, Size> VertexSizes { get; set; }

        public virtual bool NeedVertexSizes
        {
            get { return false; }
        }
    }
}