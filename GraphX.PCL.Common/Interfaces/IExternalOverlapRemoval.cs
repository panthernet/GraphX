using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;

namespace GraphX.PCL.Common.Interfaces
{
    /// <summary>
    /// Base interface for overlap removal algorithm
    /// </summary>
    /// <typeparam name="TVertex">Vertex data class</typeparam>
    public interface IExternalOverlapRemoval<TVertex>
    {
        /// <summary>
        /// Gets or sets vertices rectangle sizes 
        /// This property is filled automaticaly before calculation in GenerateGraph()/RelayoutGraph() methods
        /// </summary>
        IDictionary<TVertex, Rect> Rectangles { get; set; }
        /// <summary>
        /// Implements algorithm computation
        /// </summary>
        void Compute(CancellationToken cancellationToken);
    }
}
