namespace GraphX.Controls.Models
{
    /// <summary>
    /// Generic label factory interface
    /// </summary>
    public interface ILabelFactory<out TResult>
    {
        /// <summary>
        /// Returns newly generated label for parent control. Attachable labels will be auto attached if derived from IAttachableControl<T>
        /// </summary>
        /// <param name="control">Parent control</param>
        TResult CreateLabel<TCtrl>(TCtrl control);
    }
}