using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public class CompoundLayoutIterationEventArgs<TVertex, TEdge>
        : LayoutIterationEventArgs<TVertex, TEdge>, ICompoundLayoutIterationEventArgs<TVertex>
        where TVertex : class
        where TEdge : IEdge<TVertex>
    {
        public CompoundLayoutIterationEventArgs(
            int iteration, 
            double statusInPercent, 
            string message,
            IDictionary<TVertex, Point> vertexPositions,
            IDictionary<TVertex, Size> innerCanvasSizes)
            : base(iteration, statusInPercent, message, vertexPositions)
        {
            InnerCanvasSizes = innerCanvasSizes;
        }

        #region ICompoundLayoutIterationEventArgs<TVertex> Members

        public IDictionary<TVertex, Size> InnerCanvasSizes
        {
            get; private set;
        }

        #endregion
    }
}
