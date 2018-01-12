using GraphX.PCL.Logic.Algorithms.EdgeRouting;

namespace UAP.SimpleGraph.Models
{
    public class CurvedErParameters: EdgeRoutingParameters
    {
        /// <summary>
        /// Value by which middle edge point is offseted from original middle point
        /// </summary>
        public int VerticalCurveOffset { get; set; }
    }
}
