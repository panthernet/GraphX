using System.ComponentModel;
using GraphX;
using GraphX.PCL.Common.Models;

namespace METRO.SimpleGraph
{
    /* DataEdge is the data class for the edges. It contains all custom edge data specified by the user.
     * This class also must be derived from EdgeBase class that provides properties and methods mandatory for
     * correct GraphX operations.
     * Some of the useful EdgeBase members are:
     *  - ID property that stores unique positive identfication number. Property must be filled by user.
     *  - IsSelfLoop boolean property that indicates if this edge is self looped (eg have identical Target and Source vertices) 
     *  - RoutingPoints collection of points used to create edge routing path. If Null then straight line will be used to draw edge.
     *      In most cases it is handled automatically by GraphX.
     *  - Source property that holds edge source vertex.
     *  - Target property that holds edge target vertex.
     *  - Weight property that holds optional edge weight value that can be used in some layout algorithms.
     */

    public class DataEdge : EdgeBase<DataVertex>, INotifyPropertyChanged
    {
        /// <summary>
        /// Custom string property
        /// </summary>
        public string Text { get { return _text; } set { _text = value; OnPropertyChanged("Text"); } }
        private string _text;

        private double _visualedgethickness;
        /// <summary>
        /// Gets or sets edge thickness
        /// </summary>
        public double VisualEdgeThickness { get { return _visualedgethickness; } set { _visualedgethickness = value; OnPropertyChanged("VisualEdgeThickness"); } }
        private double _visualedgetransparency;
        /// <summary>
        /// Gets or sets edge transparency for 0 to 1
        /// </summary>
        public double VisualEdgeTransparency { get { return _visualedgetransparency; } set { _visualedgetransparency = value; OnPropertyChanged("VisualEdgeTransparency"); } }
        private string _visualcolor;
        /// <summary>
        /// Gets or sets edge visual color
        /// </summary>
        public string VisualColor { get { return _visualcolor; } set { _visualcolor = value; OnPropertyChanged("VisualColor"); } }

        /// <summary>
        /// Default constructor. We need to set at least Source and Target properties of the edge.
        /// </summary>
        /// <param name="source">Source vertex data</param>
        /// <param name="target">Target vertex data</param>
        /// <param name="weight">Optional edge weight</param>
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
			: base(source, target, weight)
		{
		}
        /// <summary>
        /// Default parameterless constructor (for serialization compatibility)
        /// </summary>
        public DataEdge()
            : base(null, null, 1)
        {
        }

        #region GET members
        public override string ToString()
        {
            return Text;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
