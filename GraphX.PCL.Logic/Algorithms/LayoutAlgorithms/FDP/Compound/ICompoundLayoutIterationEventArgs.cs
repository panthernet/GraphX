using System.Collections.Generic;
using GraphX.Measure;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public interface ICompoundLayoutIterationEventArgs<TVertex> 
        : ILayoutIterationEventArgs<TVertex>
        where TVertex : class
    {
        IDictionary<TVertex, Size> InnerCanvasSizes { get; }
    }
}
