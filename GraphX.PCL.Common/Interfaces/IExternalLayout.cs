using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;

namespace GraphX.PCL.Common.Interfaces
{
    /// <summary>
    /// Base interface for layout algorithm
    /// </summary>
    /// <typeparam name="TVertex">Vertex data class</typeparam>
    /// <typeparam name="TEdge">Edge data class</typeparam>
    public interface IExternalLayout<TVertex, in TEdge>
    {
        /// <summary>
        /// Run algorithm calculation
        /// </summary>
        void Compute(CancellationToken cancellationToken);

        /// <summary>
        /// Gets vertices positions: initial and resulting (after Compute)
        /// </summary>
        IDictionary<TVertex, Point> VertexPositions { get; }

        /// <summary>
        /// Gets or sets visual vertices sizes (autofilled if NeedVertexSizes property is set to true)
        /// </summary>
        IDictionary<TVertex, Size> VertexSizes { get; set; }

        /// <summary>
        /// Gets if algorithm needs to know visual VertexControl size (if True VertexSizes property will be filled before calculation)
        /// </summary>
        bool NeedVertexSizes { get; }

        /// <summary>
        /// Gets if algorithm supports vertex/edge freeze feature
        /// </summary>
        bool SupportsObjectFreeze { get; }

        /// <summary>
        /// Updates graph data of the existing algorithm from plain vertices and edges. Needed for easier inter-algorithm calls for complex algorithm.
        /// </summary>
        /// <param name="vertices">Vertex collection</param>
        /// <param name="edges">Edge collection</param>
        void ResetGraph(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges);
    }
}
