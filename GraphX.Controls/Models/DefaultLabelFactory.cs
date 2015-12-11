
namespace GraphX.Controls.Models
{
    /// <summary>
    /// Default label factory class
    /// </summary>
    /// <typeparam name="TLabel">Type of label to generate</typeparam>
    /// <typeparam name="TResult">Factory entity return type</typeparam>
    public class DefaultLabelFactory<TLabel, TResult> : ILabelFactory<TResult>
        where TLabel: class, new()
    {
        /// <summary>
        /// Returns newly generated label for parent control. Attachable labels will be auto attached if derived from IAttachableControl<T>
        /// </summary>
        /// <param name="control">Parent control</param>
        public virtual TResult CreateLabel<TCtrl>(TCtrl control)
        {
            var label = new TLabel();
            var aLabel = label as IAttachableControl<TCtrl>;
            if(aLabel != null)
                aLabel.Attach(control);
            return (TResult)(object)label;
        }
    }

    /// <summary>
    /// Default vertex label factory class
    /// </summary>
    public class DefaultVertexlabelFactory : DefaultLabelFactory<AttachableVertexLabelControl, IVertexLabelControl>
    {
        
    }
    /// <summary>
    /// Default edge label factory class
    /// </summary>
    public class DefaultEdgelabelFactory : DefaultLabelFactory<AttachableEdgeLabelControl, IEdgeLabelControl>
    {

    }
}
