using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;
using System;

namespace GraphX.GraphSharp.Algorithms.EdgeRouting
{
    public abstract class EdgeRoutingAlgorithmBase<TVertex, TEdge, TGraph> : IExternalEdgeRouting<TVertex, TEdge>, IDisposable
        where TVertex : class
		where TEdge : IEdge<TVertex>
        where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
        protected IEdgeRoutingParameters _parameters;
        protected TGraph _graph;
        public IDictionary<TVertex, Rect> VertexSizes { get; set; }
        public IDictionary<TVertex, Point> VertexPositions { get; set; }

		/// <summary>
		/// Gets or sets the resulting routing points of the edges.
		/// </summary>
		public IDictionary<TEdge, Point[]> EdgeRoutes
		{
			get;
			private set;
		}

        public EdgeRoutingAlgorithmBase(TGraph graph,  IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Rect> vertexSizes, IEdgeRoutingParameters parameters = null) 
		{
            _graph = graph;
            _parameters = parameters;
            VertexSizes = vertexSizes;
            VertexPositions = vertexPositions;
            EdgeRoutes = new Dictionary<TEdge, Point[]>();
		}

        /// <summary>
        /// Run algorithm calculation
        /// </summary>
        public virtual void Compute()
        {
        }

        public virtual Point[] ComputeSingle(TEdge edge)
        {
            return null;
        }


        public virtual void UpdateVertexData(TVertex vertex, Point position, Rect size)
        {
            VertexPositions[vertex] = position;
            VertexSizes[vertex] = size;
        }

        /// <summary>
        /// GraphArea rendering size
        /// </summary>
        public Rect AreaRectangle { get; set; }

        public void Dispose()
        {
            _graph = null;
            _parameters = null;
        }
    }
}