using System.Windows;
#if METRO
using Windows.UI.Xaml;
#endif

namespace GraphX.Controls.Models
{
    /// <summary>
    /// Generic label factory interface. TResult should be at least UIElement to be able to be added as the GraphArea child.
    /// </summary>
    public interface ILabelFactory<out TResult>
        where TResult: UIElement
    {
        /// <summary>
        /// Returns newly generated label for parent control. Attachable labels will be auto attached if derived from IAttachableControl<T>
        /// </summary>
        /// <param name="control">Parent control</param>
        TResult CreateLabel<TCtrl>(TCtrl control);
    }
}