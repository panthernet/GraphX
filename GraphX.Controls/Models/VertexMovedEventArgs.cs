using System.Windows;

namespace GraphX.Controls.Models
{
    public sealed class VertexMovedEventArgs : System.EventArgs
    {
        public VertexControl VertexControl { get; private set; }
        public Point Offset { get; private set; }

        public VertexMovedEventArgs(VertexControl vc, Point offset)
        {
            Offset = offset;
            VertexControl = vc;
        }
    }
}
