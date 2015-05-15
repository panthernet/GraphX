using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;

namespace GraphX.PCL.Common.Interfaces
{
    public interface IExternalOverlapRemoval<TVertex>
    {
        /// <summary>
        /// Stores vertex rectangle sizes (filled automaticaly before calculation)
        /// </summary>
        IDictionary<TVertex, Rect> Rectangles { get; set; }
        /// <summary>
        /// Implements algorithm computation
        /// </summary>
        void Compute(CancellationToken cancellationToken);
    }
}
