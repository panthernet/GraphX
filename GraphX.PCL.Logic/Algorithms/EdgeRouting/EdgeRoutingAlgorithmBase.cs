using System;
using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.EdgeRouting
{
    public abstract class EdgeRoutingAlgorithmBase<TVertex, TEdge, TGraph> : IExternalEdgeRouting<TVertex, TEdge>, IDisposable
        where TVertex : class
		where TEdge : IEdge<TVertex>
        where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
        protected IEdgeRoutingParameters Parameters;
        protected TGraph Graph;
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

        protected EdgeRoutingAlgorithmBase(TGraph graph,  IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Rect> vertexSizes, IEdgeRoutingParameters parameters = null) 
		{
            Graph = graph;
            Parameters = parameters;
            VertexSizes = vertexSizes ?? new Dictionary<TVertex, Rect>();
            VertexPositions = vertexPositions ?? new Dictionary<TVertex, Point>();
            EdgeRoutes = new Dictionary<TEdge, Point[]>();
		}

        /// <summary>
        /// Run algorithm calculation
        /// </summary>
        public virtual void Compute(CancellationToken cancellationToken)
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
            Graph = null;
            Parameters = null;
        }
    }
}