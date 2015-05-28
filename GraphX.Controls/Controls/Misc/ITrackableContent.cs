#if WPF
using System.Windows;
#elif METRO
using Windows.Foundation;
#endif
using GraphX.Controls.Models;

namespace GraphX.Controls
{
    /// <summary>
    /// Interface that represents trackable content object (e.g. provides means to notify about it's content changes)
    /// </summary>
    public interface ITrackableContent
    {
        /// <summary>
        /// Rises when content size changed
        /// </summary>
        event ContentSizeChangedEventHandler ContentSizeChanged;
        /// <summary>
        /// Gets actual content rectangle size
        /// </summary>
        Rect ContentSize { get; }
    }
}
