using System.Windows.Input;

namespace GraphX.Controls.Models
{
    public sealed class VertexSelectedEventArgs : System.EventArgs
    {
        public VertexControl VertexControl { get; private set; }
        public MouseButtonEventArgs MouseArgs { get; private set; }
        public ModifierKeys Modifiers { get; private set; }

        public VertexSelectedEventArgs(VertexControl vc, MouseEventArgs e, ModifierKeys keys)
        {
            VertexControl = vc;
            MouseArgs = e is MouseButtonEventArgs ? (MouseButtonEventArgs)e : null;
            Modifiers = keys;
        }
    }
}