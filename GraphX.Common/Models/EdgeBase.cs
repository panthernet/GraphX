using GraphX.Models.XmlSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using YAXLib;

namespace GraphX
{
    /// <summary>
    /// Base class for graph edge
    /// </summary>
    /// <typeparam name="TVertex">Vertex class</typeparam>
    public abstract class EdgeBase<TVertex>: WeightedEdge<TVertex>, IIdentifiableGraphDataObject, IRoutingInfo
    {
        public EdgeBase(TVertex source, TVertex target, double weight = 1)
            : base(source, target, weight)
        {
            ID = -1;
        }

        /// <summary>
        /// Unique edge ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Returns true if Source vertex equals Target vertex
        /// </summary>
        [YAXDontSerialize]
        public bool IsSelfLoop
        {
            get { return Source.Equals(Target); }
        }

        /// <summary>
        /// Routing points collection used to make Path visual object
        /// </summary>
        [YAXCustomSerializer(typeof(YAXPointArraySerializer))]
        public Point[] RoutingPoints { get; set; }
    }
}
