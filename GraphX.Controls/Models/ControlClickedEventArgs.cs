
using System.Windows.Input;

namespace GraphX.Controls.Models
{
    public class ControlClickedEventArgs<CType> : System.EventArgs
    {
        public CType Control { get; private set; }
        public MouseButtonEventArgs MouseArgs { get; private set; }
        public ModifierKeys Modifiers { get; private set; }

        public ControlClickedEventArgs(CType c, MouseEventArgs e, ModifierKeys keys)
        {
            Control = c;
            MouseArgs = e as MouseButtonEventArgs;
            Modifiers = keys;
        }
    }

    public sealed class VertexClickedEventArgs : ControlClickedEventArgs<VertexControl>
    {
        public VertexClickedEventArgs(VertexControl c, MouseEventArgs e, ModifierKeys keys)
            : base(c, e, keys)
        {
        }
    }

    public sealed class EdgeClickedEventArgs : ControlClickedEventArgs<EdgeControl>
    {
        public EdgeClickedEventArgs(EdgeControl c, MouseEventArgs e, ModifierKeys keys)
            : base(c, e, keys)
        {
        }
    }
}