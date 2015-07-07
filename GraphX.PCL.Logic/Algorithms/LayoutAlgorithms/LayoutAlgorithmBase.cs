using System.Collections.Generic;
using System.Linq;
using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	
	public abstract class LayoutAlgorithmBase<TVertex, TEdge, TGraph> : AlgorithmBase, ILayoutAlgorithm<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
	    // public Dictionary<TVertex, Point> FreezedVertices { get; set; } 

        /// <summary>
        /// Gets if current algorithm supports vertex freeze feature (part of VAESPS)
        /// </summary>
        public virtual bool SupportsObjectFreeze { get { return false; } }


		public IDictionary<TVertex, Point> VertexPositions { get; set; }

	    public TGraph VisitedGraph { get; set; }

	    protected LayoutAlgorithmBase( TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions = null)
	    {
	        VisitedGraph = visitedGraph;
	        VertexPositions = vertexPositions != null ? 
                new Dictionary<TVertex, Point>( vertexPositions.Where(a=> !double.IsNaN(a.Value.X)).ToDictionary(a=> a.Key, b=> b.Value) ) 
                : new Dictionary<TVertex, Point>( visitedGraph.VertexCount );
	    }

	    public IDictionary<TVertex, Size> VertexSizes { get; set; }

        public virtual bool NeedVertexSizes
        {
            get { return false; }
        }
    }
}