using System;
using Windows.Foundation;

namespace GraphX.Controls
{
    public class AreaSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Rectangle data in coordinates of content object
        /// </summary>
        public Rect Rectangle { get; set; }

        public AreaSelectedEventArgs(Rect rec)
            : base()
        {
            Rectangle = rec;
        }
    }
}
