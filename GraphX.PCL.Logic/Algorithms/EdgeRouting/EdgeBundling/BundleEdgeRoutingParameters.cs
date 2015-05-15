using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Logic.Algorithms.EdgeRouting
{
    public class BundleEdgeRoutingParameters: IEdgeRoutingParameters
    {
        /// <summary>
        /// Gets or sets the number of subdivision points each edge should have.
        /// Default value is 15.
        /// </summary>
        public int SubdivisionPoints { get; set; }

        /// <summary>
        /// Gets or sets the number of iterations for moving the control points.
        /// Default value is 250.
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether opposite edges should attracts or repulse each other.
        /// Default value is false.
        /// </summary>
        public bool RepulseOpposite { get; set; }

        /// <summary>
        /// Gets or sets the the value that determines if multiple threads should be used for the calculations.
        /// Default value is true.
        /// </summary>
        public bool UseThreading { get; set; }

        /// <summary>
        /// Gets or sets the value for the spring constant.
        /// Edges are more easely bent if the value is lower.
        /// Default value is 10.
        /// </summary>
        public float SpringConstant { get; set; }

        /// <summary>
        /// Gets or sets the treshold for the edge compatibility.
        /// Every pair of edges has the compatibility coefficient assigned to it.
        /// Range of the coefficient is from 0 to 1.
        /// Edges that have coefficient lower than the treshold between them are not considered for interaction.
        /// Default value is 0.2.
        /// </summary>
        public float Threshold { get; set; }

        /// <summary>
        /// If repulseOpposite is true, this determines how much will opposite edges repulse eachother.
        /// From -1 to 0.
        /// Default is -0.1
        /// </summary>
        public float RepulsionCoefficient { get; set; }

        /// <summary>
        /// Gets or sets the amount of straightening that will be applied after every bundling.
        /// This can produce better-looking graphs.
        /// Default value is 0.15, range is from 0 to 1.
        /// </summary>
        public float Straightening { get; set; }

        public BundleEdgeRoutingParameters()
        {
            SubdivisionPoints = 15;
            Straightening = 0.15F;
            RepulsionCoefficient = -0.1F;
            Threshold = 0.2F;
            SpringConstant = 10;
            UseThreading = true;
            RepulseOpposite = false;
            Iterations = 250;
        }
    }
}
