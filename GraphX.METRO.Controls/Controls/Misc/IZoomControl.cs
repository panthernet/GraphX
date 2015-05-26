using Windows.UI.Xaml;

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
