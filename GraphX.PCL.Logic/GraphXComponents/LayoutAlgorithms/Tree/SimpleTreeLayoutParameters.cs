namespace GraphX.GraphSharp.Algorithms.Layout.Simple.Tree
{
	public class SimpleTreeLayoutParameters : LayoutParametersBase
	{
		private double vertexGap = 10;
		/// <summary>
		/// Gets or sets the gap between the vertices.
		/// </summary>
		public double VertexGap
		{
			get { return vertexGap; }
			set
			{
				if ( vertexGap != value )
				{
					vertexGap = value;
					NotifyPropertyChanged( "VertexGap" );
				}
			}
		}

		private double layerGap = 10;
		/// <summary>
		/// Gets or sets the gap between the layers.
		/// </summary>
		public double LayerGap
		{
			get { return layerGap; }
			set
			{
				if ( layerGap != value )
				{
					layerGap = value;
					NotifyPropertyChanged( "LayerGap" );
				}
			}
		}

		private LayoutDirection direction = LayoutDirection.TopToBottom;
		/// <summary>
		/// Gets or sets the direction of the layout.
		/// </summary>
		public LayoutDirection Direction
		{
			get { return direction; }
			set
			{
				if ( direction != value )
				{
					direction = value;
					NotifyPropertyChanged( "Direction" );
				}
			}
		}

		private SpanningTreeGeneration spanningTreeGeneration = SpanningTreeGeneration.DFS;
		/// <summary>
		/// Gets or sets the direction of the layout.
		/// </summary>
		public SpanningTreeGeneration SpanningTreeGeneration
		{
			get { return spanningTreeGeneration; }
			set
			{
				if ( spanningTreeGeneration != value )
				{
					spanningTreeGeneration = value;
					NotifyPropertyChanged( "SpanningTreeGeneration" );
				}
			}
		}

        private bool optimizeWidthAndHeight = false;

        public bool OptimizeWidthAndHeight
        {
            get { return optimizeWidthAndHeight; }
            set
            {
                if (value == optimizeWidthAndHeight)
                    return;

                optimizeWidthAndHeight = value;
                NotifyPropertyChanged("OptimizeWidthAndHeight");
            }
        }

        private double widthPerHeight = 1.0;

        public double WidthPerHeight
        {
            get { return widthPerHeight; }
            set
            {
                if (value == widthPerHeight)
                    return;

                widthPerHeight = value;
                NotifyPropertyChanged("WidthPerHeight");
            }
        }
	}
}