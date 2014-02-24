using QuickGraph;
using System;

namespace GraphX
{

	public class WeightedEdge<Vertex> : IEdge<Vertex>
	{
		public double Weight { get; set; }

		public WeightedEdge(Vertex source, Vertex target)
			: this(source, target, 1) {}

		public WeightedEdge(Vertex source, Vertex target, double weight)
		{
            _source = source;
            _target = target;
			this.Weight = weight;
		}

        private Vertex _source;
        /// <summary>
        /// Source vertex data
        /// </summary>
        public Vertex Source
        {
            get { return _source; }
            set { _source = value; }
        }
        private Vertex _target;
        /// <summary>
        /// Target vertex data
        /// </summary>
        public Vertex Target
        {
            get { return _target; }
            set { _target = value; }
        }
        /// <summary>
        /// Update vertices (probably needed for serialization TODO)
        /// </summary>
        /// <param name="source">Source vertex data</param>
        /// <param name="Target">Target vertex data</param>
        public void UpdateVertices(Vertex source, Vertex Target)
        {
            _source = source; _target = Target;
        }

    }
}