using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout
{
	
	public abstract class LayoutAlgorithmBase<TVertex, TEdge, TGraph> : AlgorithmBase, ILayoutAlgorithm<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
		private readonly Dictionary<TVertex, Point> _vertexPositions;
		private readonly TGraph _visitedGraph;

       // public Dictionary<TVertex, Point> FreezedVertices { get; set; } 

        public virtual bool SupportsObjectFreeze { get { return false; } }


		public IDictionary<TVertex, Point> VertexPositions
		{
			get { return _vertexPositions; }
		}

		public TGraph VisitedGraph
		{
			get { return _visitedGraph; }
		}

	    protected LayoutAlgorithmBase( TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions = null)
	    {
	        _visitedGraph = visitedGraph;
	        _vertexPositions = vertexPositions != null ? new Dictionary<TVertex, Point>( vertexPositions ) : new Dictionary<TVertex, Point>( visitedGraph.VertexCount );
            /////FreezedVertices = new Dictionary<TVertex, Point>();
	    }

	    public IDictionary<TVertex, Size> VertexSizes { get; set; }

        public virtual bool NeedVertexSizes
        {
            get { return false; }
        }
    }
}