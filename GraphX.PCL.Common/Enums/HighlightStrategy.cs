namespace GraphX.PCL.Common.Enums
{
    public enum HighlightStrategy
    {
        /// <summary>
        /// Use existing vertex and edge controls
        /// No additional control manipulation needed
        /// </summary>
        UseExistingControls,
        /// <summary>
        /// NOT IMPLEMENTED Create new vertex and edge controls
        /// Useful when edges is not created by default
        /// </summary>
        CreateControls
    }
}
