using GraphX.PCL.Common.Enums;

namespace GraphX.PCL.Common.Interfaces
{
    public interface IIdentifiableGraphDataObject
    {
        /// <summary>
        /// Unique object identifier
        /// </summary>
        int ID { get; set; }
        /// <summary>
        /// Skip object in algorithm calc and visual control generation
        /// </summary>
        ProcessingOptionEnum SkipProcessing { get; set; }
    }
}
