using System.Windows;
using System.Windows.Input;

namespace GraphX.Models
{
    public sealed class ContentSizeChangedEventArgs : System.EventArgs
    {
        public Rect OldSize { get; private set; }
        public Rect NewSize { get; private set; }

        public ContentSizeChangedEventArgs(Rect oldSize, Rect newSize)
            : base()
        {
            OldSize = oldSize;
            NewSize = newSize;
        }
    }
}
