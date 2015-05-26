using System;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Common.Models
{
    public abstract class VertexBase: IGraphXVertex
    {
        /// <summary>
        /// Skip vertex in algo calc and visualization
        /// </summary>
        public ProcessingOptionEnum SkipProcessing { get; set; }

        protected VertexBase()
        {
            ID = -1;
        }
        /// <summary>
        /// Unique vertex ID
        /// </summary>
        public int ID { get; set; }

        public bool Equals(IGraphXVertex other)
        {
            return Equals(this, other);
        }
    }
}
