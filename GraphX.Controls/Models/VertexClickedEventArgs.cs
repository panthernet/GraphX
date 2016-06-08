
using System.Windows.Input;

namespace GraphX.Controls.Models
{
    public sealed class VertexClickedEventArgs : System.EventArgs
    {
        public VertexControl VertexControl { get; private set; }
        public MouseButtonEventArgs MouseArgs { get; private set; }
        public ModifierKeys Modifiers { get; private set; }

        public VertexClickedEventArgs(VertexControl vc, MouseEventArgs e, ModifierKeys keys)
        {
            VertexControl = vc;
            MouseArgs = e as MouseButtonEventArgs;
            Modifiers = keys;
        }
    }
}