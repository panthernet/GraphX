using System.Windows.Input;

namespace GraphX.Models
{
    public sealed class EdgeSelectedEventArgs : System.EventArgs
    {
        public EdgeControl EdgeControl { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public MouseButtonEventArgs MouseArgs { get; set; }

        public EdgeSelectedEventArgs(EdgeControl ec, MouseButtonEventArgs e, ModifierKeys keys)
            : base()
        {
            EdgeControl = ec;
            Modifiers = keys;
            MouseArgs = e;
        }
    }
}
