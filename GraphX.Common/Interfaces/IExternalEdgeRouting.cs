using System.Collections.Generic;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.EdgeRouting
{
    public interface IExternalEdgeRouting<TVertex, TEdge>
    {
        /// <summary>
        /// Run algorithm calculation
        /// </summary>
        void Compute();
        /// <summary>
        /// Compute edge routing for single edge
        /// </summary>
        /// <param name="edge">Supplied edge data</param>
        Point[] ComputeSingle(TEdge edge);

        /// <summary>
        /// Update data stored in algorithm for specified vertex
        /// </summary>
        /// <param name="vertex">Data vertex</param>
        /// <param name="position">Vertex position</param>
        /// <param name="size">Vertex size</param>
        void UpdateVertexData(TVertex vertex, Point position, Rect size);

        /// <summary>
        /// Get visual vertex sizes (autofilled before Compute() call)
        /// </summary>
        IDictionary<TVertex, Rect> VertexSizes { get; set; }

        /// <summary>
        /// Get visual vertex positions (autofilled before Compute() call)
        /// </summary>
        IDictionary<TVertex, Point> VertexPositions { get; set; }

        /// <summary>
        /// Get resulting edge routes collection 
        /// </summary>
        IDictionary<TEdge, Point[]> EdgeRoutes { get; }

        /// <summary>
        /// GraphArea rendering size
        /// </summary>
        Rect AreaRectangle { get; set; }

    }
}
