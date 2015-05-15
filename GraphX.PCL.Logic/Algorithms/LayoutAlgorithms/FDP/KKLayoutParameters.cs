namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public class KKLayoutParameters : LayoutParametersBase
	{
		private double _width = 300;
		/// <summary>
		/// Width of the bounding box.
		/// </summary>
		public double Width
		{
			get { return _width; }
			set
			{
				_width = value;
				NotifyPropertyChanged("Width");
			}
		}

		private double _height = 300;
		/// <summary>
		/// Height of the bounding box.
		/// </summary>
		public double Height
		{
			get { return _height; }
			set
			{
				_height = value;
				NotifyPropertyChanged("Height");
			}
		}

		private int _maxIterations = 200;
		/// <summary>
		/// Maximum number of the iterations.
		/// </summary>
		public int MaxIterations
		{
			get { return _maxIterations; }
			set
			{
				_maxIterations = value;
				NotifyPropertyChanged("MaxIterations");
			}
		}

		private double _k = 1;
		public double K
		{
			get { return _k; }
			set
			{
				_k = value;
				NotifyPropertyChanged("K");
			}
		}


		private bool _adjustForGravity;
		/// <summary>
		/// If true, then after the layout process, the vertices will be moved, so the barycenter will be
		/// in the center point of the bounding box.
		/// </summary>
		public bool AdjustForGravity
		{
			get { return _adjustForGravity; }
			set
			{
				_adjustForGravity = value;
				NotifyPropertyChanged("AdjustForGravity");
			}
		}

		private bool _exchangeVertices;
		public bool ExchangeVertices
		{
			get { return _exchangeVertices; }
			set
			{
				_exchangeVertices = value;
				NotifyPropertyChanged("ExchangeVertices");
			}
		}

		private double _lengthFactor = 1;
		/// <summary>
		/// Multiplier of the ideal edge length. (With this parameter the user can modify the ideal edge length).
		/// </summary>
		public double LengthFactor
		{
			get { return _lengthFactor; }
			set
			{
				_lengthFactor = value;
				NotifyPropertyChanged("LengthFactor");
			}
		}

		private double _disconnectedMultiplier = 0.5;
		/// <summary>
		/// Ideal distance between the disconnected points (1 is equal the ideal edge length).
		/// </summary>
		public double DisconnectedMultiplier
		{
			get { return _disconnectedMultiplier; }
			set
			{
				_disconnectedMultiplier = value;
				NotifyPropertyChanged("DisconnectedMultiplier");
			}
		}
	}
}