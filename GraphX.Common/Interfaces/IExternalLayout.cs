using System.Collections.Generic;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.Layout
{
    public interface IExternalLayout<TVertex>
    {
        /// <summary>
        /// Run algorithm calculation
        /// </summary>
        void Compute();

        /// <summary>
        /// Vertex positions: initial and resulting (after Compute)
        /// </summary>
        IDictionary<TVertex, Point> VertexPositions { get; }

        /// <summary>
        /// Stores visual vertex sizes (autofilled if NeedVertexSizes property is set to true)
        /// </summary>
        IDictionary<TVertex, Size> VertexSizes { get; set; }

        /// <summary>
        /// If algorithm needs to know visual vertex control sizes they will be set into VertexSizes property before calculation
        /// </summary>
        bool NeedVertexSizes { get; }
    }
}
