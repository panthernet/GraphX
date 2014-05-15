using System.Collections.Generic;
using GraphX.Measure;

namespace GraphX.GraphSharp.Algorithms.Layout.Compound
{
    public interface ICompoundLayoutIterationEventArgs<TVertex> 
        : ILayoutIterationEventArgs<TVertex>
        where TVertex : class
    {
        IDictionary<TVertex, Size> InnerCanvasSizes { get; }
    }
}
