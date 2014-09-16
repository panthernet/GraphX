using Windows.UI.Xaml.Input;
using GraphX.Controls;

namespace GraphX.Models
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
