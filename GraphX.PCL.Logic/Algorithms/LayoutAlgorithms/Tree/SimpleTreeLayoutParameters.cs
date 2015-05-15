namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public class SimpleTreeLayoutParameters : LayoutParametersBase
	{
		private double _vertexGap = 10;
		/// <summary>
		/// Gets or sets the gap between the vertices.
		/// </summary>
		public double VertexGap
		{
			get { return _vertexGap; }
			set
			{
			    if (_vertexGap == value) return;
			    _vertexGap = value;
			    NotifyPropertyChanged( "VertexGap" );
			}
		}

		private double _layerGap = 10;
		/// <summary>
		/// Gets or sets the gap between the layers.
		/// </summary>
		public double LayerGap
		{
			get { return _layerGap; }
			set
			{
			    if (_layerGap == value) return;
			    _layerGap = value;
			    NotifyPropertyChanged( "LayerGap" );
			}
		}

		private LayoutDirection _direction = LayoutDirection.TopToBottom;
		/// <summary>
		/// Gets or sets the direction of the layout.
		/// </summary>
		public LayoutDirection Direction
		{
			get { return _direction; }
			set
			{
			    if (_direction == value) return;
			    _direction = value;
			    NotifyPropertyChanged( "Direction" );
			}
		}

		private SpanningTreeGeneration _spanningTreeGeneration = SpanningTreeGeneration.DFS;
		/// <summary>
		/// Gets or sets the direction of the layout.
		/// </summary>
		public SpanningTreeGeneration SpanningTreeGeneration
		{
			get { return _spanningTreeGeneration; }
			set
			{
			    if (_spanningTreeGeneration == value) return;
			    _spanningTreeGeneration = value;
			    NotifyPropertyChanged( "SpanningTreeGeneration" );
			}
		}

        private bool _optimizeWidthAndHeight;

        public bool OptimizeWidthAndHeight
        {
            get { return _optimizeWidthAndHeight; }
            set
            {
                if (value == _optimizeWidthAndHeight)
                    return;

                _optimizeWidthAndHeight = value;
                NotifyPropertyChanged("OptimizeWidthAndHeight");
            }
        }

        private double _widthPerHeight = 1.0;

        public double WidthPerHeight
        {
            get { return _widthPerHeight; }
            set
            {
                if (value == _widthPerHeight)
                    return;

                _widthPerHeight = value;
                NotifyPropertyChanged("WidthPerHeight");
            }
        }
	}
}