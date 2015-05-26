using System.Windows;
using GraphX.Controls.Models;

namespace GraphX.Controls
{
    /// <summary>
    /// Interface that represents trackable content object
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
