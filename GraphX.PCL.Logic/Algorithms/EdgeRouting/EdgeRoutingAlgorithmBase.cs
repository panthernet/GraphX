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

        /// <summary>
        /// Gets or sets visual vertices sizes (autofilled before Compute() call)
        /// </summary>
        public IDictionary<TVertex, Rect> VertexSizes { get; set; }

        /// <summary>
        /// Gets or sets visual vertices positions (autofilled before Compute() call)
        /// </summary>
        public IDictionary<TVertex, Point> VertexPositions { get; set; }

		/// <summary>
		/// Gets or sets the resulting routing points of the edges.
		/// </summary>
		public IDictionary<TEdge, Point[]> EdgeRoutes
		{
			get;
			private set;
		}

        /// <summary>
        /// Base class for edge routing algorithms
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="vertexPositions">Vertex positions dictionary</param>
        /// <param name="vertexSizes">Vertex sizes dictionary</param>
        /// <param name="parameters">Algorithm parameters</param>
        protected EdgeRoutingAlgorithmBase(TGraph graph,  IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Rect> vertexSizes, IEdgeRoutingParameters parameters = null) 
		{
            Graph = graph;
            Parameters = parameters;
            VertexSizes = vertexSizes ?? new Dictionary<TVertex, Rect>();
            VertexPositions = vertexPositions ?? new Dictionary<TVertex, Point>();
            EdgeRoutes = new Dictionary<TEdge, Point[]>();
		}

        /// <summary>
        /// Returns algorithm parameters as specified type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public T GetParameters<T>()
            where T: class
        {
            return Parameters as T;
        }

        /// <summary>
        /// Run algorithm calculation
        /// </summary>
        public abstract void Compute(CancellationToken cancellationToken);

        /// <summary>
        /// Compute edge routing for single edge
        /// </summary>
        /// <param name="edge">Supplied edge data</param>
        public abstract Point[] ComputeSingle(TEdge edge);


        /// <summary>
        /// Update data stored in algorithm for specified vertex
        /// </summary>
        /// <param name="vertex">Data vertex</param>
        /// <param name="position">Vertex position</param>
        /// <param name="size">Vertex size</param>
        public virtual void UpdateVertexData(TVertex vertex, Point position, Rect size)
        {
            VertexPositions[vertex] = position;
            VertexSizes[vertex] = size;
        }

        /// <summary>
        /// GraphArea rendering size
        /// </summary>
        public Rect AreaRectangle { get; set; }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Graph = null;
            Parameters = null;
        }
    }
}