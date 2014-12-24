using GraphX.PCL.Common.Enums;

namespace GraphX
{
    public abstract class VertexBase: IGraphXVertex
    {
        /// <summary>
        /// Skip vertex in algo calc and visualization
        /// </summary>
        public ProcessingOptionEnum SkipProcessing { get; set; }

        public VertexBase()
        {
            ID = -1;
        }
        /// <summary>
        /// Unique vertex ID
        /// </summary>
        public int ID { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as VertexBase;
            if (o == null) return false;
            else return ID == o.ID;
        }

        public override int GetHashCode()
        {
            return (int)SkipProcessing ^ ID;
        }

        public bool Equals(IGraphXVertex other)
        {
            return Equals(this, other);
        }
    }
}
