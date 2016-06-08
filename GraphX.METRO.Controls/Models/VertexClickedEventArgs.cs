using Windows.UI.Xaml.Input;

namespace GraphX.Controls.Models
{
    public sealed class VertexClickedEventArgs : System.EventArgs
    {
        public VertexControl VertexControl { get; private set; }
        public PointerRoutedEventArgs Args { get; private set; }

        public VertexClickedEventArgs(VertexControl vc, PointerRoutedEventArgs e)
        {
            VertexControl = vc;
            Args = e;
        }
    }
}