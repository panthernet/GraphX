using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Common.Models
{

    public class WeightedEdge<TVertex> : IWeightedEdge<TVertex>
	{
		public double Weight { get; set; }

		public WeightedEdge(TVertex source, TVertex target)
			: this(source, target, 1) {}

		public WeightedEdge(TVertex source, TVertex target, double weight)
		{
            Source = source;
            Target = target;
			Weight = weight;
		}

        /// <summary>
        /// Source vertex data
        /// </summary>
        public TVertex Source { get; set; }

        /// <summary>
        /// Target vertex data
        /// </summary>
        public TVertex Target { get; set; }

        /// <summary>
        /// Update vertices (probably needed for serialization TODO)
        /// </summary>
        /// <param name="source">Source vertex data</param>
        /// <param name="target">Target vertex data</param>
        public void UpdateVertices(TVertex source, TVertex target)
        {
            Source = source; Target = target;
        }

    }
}