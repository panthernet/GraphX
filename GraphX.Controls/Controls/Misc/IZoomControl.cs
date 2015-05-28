#if WPF
using System.Windows;
#elif METRO
using Windows.UI.Xaml;
#endif

namespace GraphX.Controls
{
    /// <summary>
    /// Common imterface for all possible zoomcontrol objects
    /// </summary>
    public interface IZoomControl
    {
        UIElement PresenterVisual { get; }
    }
}
