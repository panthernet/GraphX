using Windows.UI.Xaml.Input;

namespace GraphX.Controls.Models
{
    public class EdgeSelectedEventArgs : System.EventArgs
    {
        public EdgeControl EdgeControl { get; set; }
        public PointerRoutedEventArgs Args { get; set; }

        public EdgeSelectedEventArgs(EdgeControl ec, PointerRoutedEventArgs e)
            : base()
        {
            EdgeControl = ec;
            Args = e;
        }
    }

    public sealed class EdgeLabelSelectedEventArgs : EdgeSelectedEventArgs
    {
        public IEdgeLabelControl EdgeLabelControl { get; set; }


        public EdgeLabelSelectedEventArgs(IEdgeLabelControl label, EdgeControl ec, PointerRoutedEventArgs e, object nu = null)
            : base(ec, e)
        {
            EdgeLabelControl = label;
        }
    }
}
