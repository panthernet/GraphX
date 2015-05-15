namespace GraphX.PCL.Common.Enums
{
    /// <summary>
    /// Built-in layout algorithm types
    /// </summary>
    public enum LayoutAlgorithmTypeEnum
    {
        BoundedFR,
        Circular,
        CompoundFDP,
        EfficientSugiyama,
        Sugiyama,
        FR,
        ISOM,
        KK,
        LinLog,
        Tree,
        /// <summary>
        /// Simple random vertices layout
        /// </summary>
        SimpleRandom,
        //BalloonTree
        /// <summary>
        /// Do not perform any layout. Layout will be manualy managed by end-user.
        /// </summary>
        Custom,
    }
}