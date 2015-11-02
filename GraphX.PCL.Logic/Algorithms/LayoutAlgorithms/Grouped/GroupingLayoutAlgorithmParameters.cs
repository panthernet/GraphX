using System.Collections.Generic;
using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms.Grouped
{
    public class GroupingLayoutAlgorithmParameters<TVertex, TEdge> : LayoutParametersBase
        where TVertex : class, IGraphXVertex
        where TEdge : IGraphXEdge<TVertex>
    {
        /// <summary>
        /// If true runs layouting pass for resulting group zone rectangles (auto calc if not specified) to eliminate overlaps
        /// </summary>
        public bool ArrangeGroups { get; set; }

        /// <summary>
        /// Gets or sets minimum horizontal gap between arranged groups. If ArrangeGroups = True.
        /// </summary>
        public bool ArrangeHorizontalGap { get; set; }

        /// <summary>
        /// Gets or sets minimum vertical gap between arranged groups. If ArrangeGroups = True.
        /// </summary>
        public bool ArrangeVerticalGap { get; set; }

        /// <summary>
        /// Gets or sets overlap removal algorithm to use for vertex groups
        /// </summary>
        public IOverlapRemovalAlgorithm<object, IOverlapRemovalParameters> OverlapRemovalAlgorithm { get; set; }

        /// <summary>
        /// Gets ort sets parameters list containing parameters for vertex groups
        /// </summary>
        public List<AlgorithmGroupParameters<TVertex, TEdge>> GroupParametersList { get; set; }

        public GroupingLayoutAlgorithmParameters()
        {
            GroupParametersList = new List<AlgorithmGroupParameters<TVertex, TEdge>>();
        }

        /// <summary>
        /// Creates grouping algorithm parameters
        /// </summary>
        /// <param name="paramsList">Params list</param>
        /// <param name="arrangeGroups">Arrange groups of vertices on the last step to exclude group overlaps</param>
        public GroupingLayoutAlgorithmParameters(List<AlgorithmGroupParameters<TVertex, TEdge>> paramsList, bool arrangeGroups = false)
            : this()
        {
            GroupParametersList = paramsList;
            ArrangeGroups = arrangeGroups;
        }
    }
}