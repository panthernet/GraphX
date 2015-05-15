using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Logic.Algorithms.EdgeRouting
{
    public class SimpleERParameters: IEdgeRoutingParameters
    {
        /// <summary>
        /// Get or set side step value when searching for way around vertex
        /// </summary>
        public double SideStep { get; set; }
        /// <summary>
        /// Get or set backward step when intersection is met
        /// </summary>
        public double BackStep { get; set; }

        public SimpleERParameters()
        {
            SideStep = 5;
            BackStep = 10;
        }
    }
}
