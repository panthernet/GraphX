using System.Windows;
using System.Windows.Input;

namespace GraphX.Models
{
    public sealed class VertexSelectedEventArgs : System.EventArgs
    {
        public VertexControl VertexControl { get; private set; }
        public MouseButtonEventArgs MouseArgs { get; private set; }

        public VertexSelectedEventArgs(VertexControl vc, MouseButtonEventArgs e)
            : base()
        {
            VertexControl = vc;
            MouseArgs = e;
        }
    }
}