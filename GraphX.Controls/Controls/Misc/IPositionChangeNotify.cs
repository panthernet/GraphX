namespace GraphX.Controls
{
    /// <summary>
    /// Interface that implements means to notify on objetc position changes (implemented to track attached property changes parent -> child)
    /// </summary>
    public interface IPositionChangeNotify
    {
#if METRO
        /// <summary>
        /// Notify object that it's position within container has been changed
        /// </summary>
        void OnPositionChanged();
#endif
    }
}
