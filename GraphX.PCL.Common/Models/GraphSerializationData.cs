using GraphX.Measure;

namespace GraphX.PCL.Common.Models
{
    public class GraphSerializationData
    {
        /// <summary>
        /// Gets or sets graph data object
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// Gets or sets control position
        /// </summary>
        public Point Position { get; set; }
        /// <summary>
        /// Gets or sets control visibility
        /// </summary>
        public bool IsVisible { get; set; } = true;
        /// <summary>
        /// Gets or sets control label availability
        /// </summary>
        public bool HasLabel { get; set; }
    }
}
