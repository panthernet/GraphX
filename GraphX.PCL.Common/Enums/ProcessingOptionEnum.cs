namespace GraphX.PCL.Common.Enums
{
    /// <summary>
    /// Specifies how object will be treated in calculations and visualization
    /// </summary>
    public enum ProcessingOptionEnum
    {
        /// <summary>
        /// Process object as intended
        /// </summary>
        Default,
        /// <summary>
        /// Freeze object so its position will remain intact for all subsequent calculations.
        /// </summary>
        Freeze,
        /// <summary>
        /// Exclude object from all consequent calulations
        /// </summary>
        Exclude
    }
}
