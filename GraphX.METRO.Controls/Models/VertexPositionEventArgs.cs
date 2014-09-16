using System;
using Windows.Foundation;
using GraphX.Controls;

namespace GraphX.Models
{
    public sealed class VertexPositionEventArgs : EventArgs
    {
        /// <summary>
        /// Vertex control
        /// </summary>
        public VertexControl VertexControl { get; private set; }
        /// <summary>
        /// Attached coordinates X and Y 
        /// </summary>
        public Point Position { get; private set; }
        /// <summary>
        /// Offset of the vertex control within the GraphArea
        /// </summary>
        public Point OffsetPosition { get; private set; }

        public VertexPositionEventArgs(Point offset, Point pos, VertexControl vc)
        {
            OffsetPosition = offset;
            VertexControl = vc;
            Position = pos;
        }
    }
}
