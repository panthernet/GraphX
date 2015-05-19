using System.Windows.Media;
using GraphX;
using GraphX.PCL.Common.Models;

namespace ShowcaseApp.WPF
{
    public class DataVertex: VertexBase
    {
        public string Text { get; set; }
        public string Name { get; set; }
        public string Profession { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public int ImageId { get; set; }

        #region Calculated or static props

        public override string ToString()
        {
            return Text;
        }

        #endregion

        /// <summary>
        /// Default constructor for this class
        /// (required for serialization).
        /// </summary>
        public DataVertex():this(string.Empty)
        {
        }

        public DataVertex(string text = "")
        {
            Text = string.IsNullOrEmpty(text) ? "New Vertex" : text;
        }
    }
}
