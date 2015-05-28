#if WPF
using System.Windows;
#elif METRO
using Windows.Foundation;
#endif

namespace GraphX.Controls.Models
{
    public sealed class ContentSizeChangedEventArgs : System.EventArgs
    {
        public Rect OldSize { get; private set; }
        public Rect NewSize { get; private set; }

        public ContentSizeChangedEventArgs(Rect oldSize, Rect newSize)
        {
            OldSize = oldSize;
            NewSize = newSize;
        }
    }
}
