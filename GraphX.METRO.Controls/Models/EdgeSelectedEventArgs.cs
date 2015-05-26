using Windows.UI.Xaml.Input;

namespace GraphX.Controls.Models
{
    public sealed class EdgeSelectedEventArgs : System.EventArgs
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
}
