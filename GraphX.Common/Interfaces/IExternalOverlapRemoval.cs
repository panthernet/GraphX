using System.Collections.Generic;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
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
        void Compute();
    }
}
