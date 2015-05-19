using System.Collections.Generic;
using GraphX.Measure;

namespace GraphX.PCL.Common.Interfaces
{
    /// <summary>
    /// If added to layout algorithm specifies that it uses it's own edge routing and thus
    /// should ignore edge routing algorithm
    /// </summary>
    /// <typeparam name="TEdge">Edge type</typeparam>
    public interface ILayoutEdgeRouting<TEdge>
    {       
        /// <summary>
        /// Get resulting edge routes collection 
        /// </summary>
        IDictionary<TEdge, Point[]> EdgeRoutes { get; }
    }
}
