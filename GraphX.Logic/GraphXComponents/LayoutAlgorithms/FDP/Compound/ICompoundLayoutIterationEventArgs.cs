using System.Collections.Generic;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.Layout.Compound
{
    public interface ICompoundLayoutIterationEventArgs<TVertex> 
        : ILayoutIterationEventArgs<TVertex>
        where TVertex : class
    {
        IDictionary<TVertex, Size> InnerCanvasSizes { get; }
    }
}
