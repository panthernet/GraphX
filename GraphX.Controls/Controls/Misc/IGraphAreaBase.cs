#if WPF
using System.Windows;
#else
using Windows.Foundation;
#endif

namespace GraphX.Controls
{
    /// <summary>
    /// Common GraphArea interface
    /// </summary>
    public interface IGraphAreaBase
    {
        void SetPrintMode(bool value, bool offsetControls = true, int margin = 0);

        Rect ContentSize { get; }
    }
}
