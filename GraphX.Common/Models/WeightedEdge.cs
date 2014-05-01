using QuickGraph;
using System;

namespace GraphX
{

    public class WeightedEdge<TVertex> : IWeightedEdge<TVertex>
	{
		public double Weight { get; set; }

		public WeightedEdge(TVertex source, TVertex target)
			: this(source, target, 1) {}

		public WeightedEdge(TVertex source, TVertex target, double weight)
		{
            _source = source;
            _target = target;
			this.Weight = weight;
		}

        private TVertex _source;
        /// <summary>
        /// Source vertex data
        /// </summary>
        public TVertex Source
        {
            get { return _source; }
            set { _source = value; }
        }
        private TVertex _target;
        /// <summary>
        /// Target vertex data
        /// </summary>
        public TVertex Target
        {
            get { return _target; }
            set { _target = value; }
        }
        /// <summary>
        /// Update vertices (probably needed for serialization TODO)
        /// </summary>
        /// <param name="source">Source vertex data</param>
        /// <param name="Target">Target vertex data</param>
        public void UpdateVertices(TVertex source, TVertex Target)
        {
            _source = source; _target = Target;
        }

    }
}