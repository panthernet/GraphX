using System;

namespace GraphX
{
    public abstract class VertexBase: IGraphXVertex
    {
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
            return base.GetHashCode();
        }

        public bool Equals(IGraphXVertex other)
        {
            return this == other;
        }
    }
}
