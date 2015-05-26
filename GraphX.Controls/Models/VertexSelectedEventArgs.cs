using System.Windows.Input;

namespace GraphX.Controls.Models
{
    public sealed class VertexSelectedEventArgs : System.EventArgs
    {
        public VertexControl VertexControl { get; private set; }
        public MouseButtonEventArgs MouseArgs { get; private set; }
        public ModifierKeys Modifiers { get; private set; }

        public VertexSelectedEventArgs(VertexControl vc, MouseButtonEventArgs e, ModifierKeys keys)
        {
            VertexControl = vc;
            MouseArgs = e;
            Modifiers = keys;
        }
    }
}