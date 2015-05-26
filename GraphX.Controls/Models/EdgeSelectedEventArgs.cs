using System.Windows.Input;

namespace GraphX.Controls.Models
{
    public sealed class EdgeSelectedEventArgs : System.EventArgs
    {
        public EdgeControl EdgeControl { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public MouseButtonEventArgs MouseArgs { get; set; }

        public EdgeSelectedEventArgs(EdgeControl ec, MouseButtonEventArgs e, ModifierKeys keys)
        {
            EdgeControl = ec;
            Modifiers = keys;
            MouseArgs = e;
        }
    }
}
