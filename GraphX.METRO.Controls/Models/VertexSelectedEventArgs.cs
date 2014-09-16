using Windows.UI.Xaml.Input;
using GraphX.Controls;

namespace GraphX.Models
{
    public sealed class VertexSelectedEventArgs : System.EventArgs
    {
        public VertexControl VertexControl { get; private set; }
        public PointerRoutedEventArgs Args { get; private set; }

        public VertexSelectedEventArgs(VertexControl vc, PointerRoutedEventArgs e)
            : base()
        {
            VertexControl = vc;
            Args = e;
        }
    }
}