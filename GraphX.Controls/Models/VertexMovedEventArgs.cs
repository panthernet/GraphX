using System.Windows;
using System.Windows.Input;

namespace GraphX.Controls.Models
{
    public sealed class VertexMovedEventArgs : System.EventArgs
    {
        public VertexControl VertexControl { get; private set; }
        public Point Offset { get; private set; }
        public MouseEventArgs Args { get; private set; }

        public VertexMovedEventArgs(VertexControl vc, MouseEventArgs e)
        {
            Offset = new Point();
            VertexControl = vc;
            Args = e;
        }
    }
}
