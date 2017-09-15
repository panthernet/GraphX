namespace GraphX.Controls.Models
{
    /// <summary>
    /// Represents attachable control so it can be automaticaly attached to parent entity
    /// </summary>
    /// <typeparam name="T">Parent entity type</typeparam>
    public interface IAttachableControl<in T>
    {
        /// <summary>
        /// Attach control to parent entity
        /// </summary>
        /// <param name="control">Parent entity</param>
        void Attach(T control);
        /// <summary>
        /// Detach label from control
        /// </summary>
        void Detach();
    }
}