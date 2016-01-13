using System.Windows;
#if METRO
using Windows.UI.Xaml;
#endif

namespace GraphX.Controls.Models
{
    /// <summary>
    /// Default label factory class
    /// </summary>
    /// <typeparam name="TLabel">Type of label to generate. Should be UIElement derived.</typeparam>
    public abstract class DefaultLabelFactory<TLabel> : ILabelFactory<TLabel>
        where TLabel: UIElement, new()
    {
        /// <summary>
        /// Returns newly generated label for parent control. Attachable labels will be auto attached if derived from IAttachableControl<TCtrl>
        /// </summary>
        /// <param name="control">Parent control</param>
        public virtual TLabel CreateLabel<TCtrl>(TCtrl control)
        {
            var label = new TLabel();
            var aLabel = label as IAttachableControl<TCtrl>;
            if(aLabel != null)
                aLabel.Attach(control);
            return label;
        }
    }

    /// <summary>
    /// Default vertex label factory class
    /// </summary>
    public class DefaultVertexlabelFactory : DefaultLabelFactory<AttachableVertexLabelControl>
    {
        
    }
    /// <summary>
    /// Default edge label factory class
    /// </summary>
    public class DefaultEdgelabelFactory : DefaultLabelFactory<AttachableEdgeLabelControl>
    {

    }
}
