using System.ComponentModel;
using GraphX;
using GraphX.PCL.Common.Models;

namespace METRO.SimpleGraph
{
    /* DataVertex is the data class for the vertices. It contains all custom vertex data specified by the user.
     * This class also must be derived from VertexBase that provides properties and methods mandatory for
     * correct GraphX operations.
     * Some of the useful VertexBase members are:
     *  - ID property that stores unique positive identfication number. Property must be filled by user.
     *  
     */

    public class DataVertex: VertexBase, INotifyPropertyChanged
    {

        /// <summary>
        /// Vertex label text
        /// </summary>
        public string LabelText { get { return _labeltext; } set { _labeltext = value; OnPropertyChanged("LabelText"); } }
        private string _labeltext;

        private bool _visualselected;
        public bool VisualSelected { get { return _visualselected; } set { _visualselected = value; OnPropertyChanged("VisualSelected"); } }

        /// <summary>
        /// Controls overall vertex diameter
        /// </summary>
        public int VisualDiameter { get { return _visualdiameter; } set { _visualdiameter = value; OnPropertyChanged("VisualDiameter"); } }
        private int _visualdiameter;

        /// <summary>
        /// Controls inner circle diameter
        /// </summary>
        public int VisualInnerDiameter { get { return _visualinnerdiameter; } set { _visualinnerdiameter = value; OnPropertyChanged("VisualInnerDiameter"); } }
        private int _visualinnerdiameter;

        /// <summary>
        /// Controls the thickness of vertex outer ring
        /// </summary>
        public double VisualOuterRingThickness { get { return 5d; } }


        #region Calculated or static props

        public override string ToString()
        {
            return LabelText;
        }

        #endregion

        /// <summary>
        /// Default parameterless constructor for this class
        /// (required for YAXLib serialization)
        /// </summary>
        public DataVertex():this("")
        {
        }

        public DataVertex(string text = "")
        {
            LabelText = text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
