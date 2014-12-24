using GraphX.PCL.Common.Enums;

namespace GraphX
{
    public interface IIdentifiableGraphDataObject
    {
        /// <summary>
        /// Unique object identifier
        /// </summary>
        int ID { get; set; }
        /// <summary>
        /// Skip edge in algorithm calc and visual control generation
        /// </summary>
        ProcessingOptionEnum SkipProcessing { get; set; }
    }
}
