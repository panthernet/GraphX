using Windows.UI.Xaml;

namespace GraphX.METRO.Controls
{
    /// <summary>
    /// Common imterface for all possible zoomcontrol objects
    /// </summary>
    public interface IZoomControl
    {
        UIElement PresenterVisual { get; }
    }
}
