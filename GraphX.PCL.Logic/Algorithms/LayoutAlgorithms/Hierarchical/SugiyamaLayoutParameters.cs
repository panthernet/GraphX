namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public enum PositionCalculationMethodTypes
	{
		/// <summary>
		/// Barycenters of the vertices computed based on the 
		/// indexes of the vertices.
		/// </summary>
		IndexBased,

		/// <summary>
		/// Barycenters of the vertices computed based on
		/// the vertex sizes and positions.
		/// </summary>
		PositionBased
	}

	/// <summary>
	/// Parameters of the Sugiyama layout.
	/// </summary>
	public class SugiyamaLayoutParameters : LayoutParametersBase
	{
		#region Helper Types
		public enum PromptingConstraintType
		{
			Compulsory,
			Recommendation,
			Irrelevant
		}
		#endregion

		internal float  Horizontalgap = 10;
		internal float  Verticalgap = 10;
		private bool    _dirty = true;
		private int     _phase1IterationCount = 8;
		private int     _phase2IterationCount = 5;
		private bool    _minimizeHierarchicalEdgeLong = true;
		private PositionCalculationMethodTypes _positionCalculationMethod = PositionCalculationMethodTypes.PositionBased;
		private bool	_simplify = true;
		private bool	_baryCenteringByPosition;
		private PromptingConstraintType _promptingConstraint = PromptingConstraintType.Recommendation;
		private float _maxWidth = float.MaxValue;

		/// <summary>
		/// Minimal horizontal gap between the vertices.
		/// </summary>
		public float HorizontalGap
		{
			get { return Horizontalgap; }
			set
			{
				if ( Horizontalgap != value )
				{
					Horizontalgap = value;
					NotifyPropertyChanged( "HorizontalGap" );
				}
			}
		}

		public float MaxWidth
		{
			get { return _maxWidth; }
			set
			{
				if ( _maxWidth != value )
				{
					_maxWidth = value;
					NotifyPropertyChanged( "MaxWidth" );
				}
			}
		}

		public bool BaryCenteringByPosition
		{
			get { return _baryCenteringByPosition; }
			set
			{
				if ( _baryCenteringByPosition != value )
				{
					_baryCenteringByPosition = value;
					NotifyPropertyChanged( "BaryCenteringByPosition" );
				}
			}
		}

		/// <summary>
		/// Minimal vertical gap between the vertices.
		/// </summary>
		public float VerticalGap
		{
			get { return Verticalgap; }
			set
			{
				if ( Verticalgap != value )
				{
					Verticalgap = value;
					NotifyPropertyChanged( "VerticalGap" );
				}
			}
		}

		/// <summary>
		/// Start with a dirty round (allow to increase the number of the edge-crossings, but 
		/// try to put the vertices to it's barycenter).
		/// </summary>
		public bool DirtyRound
		{
			get { return _dirty; }
			set
			{
				if ( _dirty != value )
				{
					_dirty = value;
					NotifyPropertyChanged( "DirtyRound" );
				}
			}
		}

		/// <summary>
		/// Maximum iteration count in the Phase 1 of the Sugiyama algo.
		/// </summary>
		public int Phase1IterationCount
		{
			get { return _phase1IterationCount; }
			set
			{
				if ( _phase1IterationCount != value )
				{
					_phase1IterationCount = value;
					NotifyPropertyChanged( "Phase1IterationCount" );
				}
			}
		}

		/// <summary>
		/// Maximum iteration count in the Phase 2 of the Sugiyama algo.
		/// </summary>
		public int Phase2IterationCount
		{
			get { return _phase2IterationCount; }
			set
			{
				if ( _phase2IterationCount != value )
				{
					_phase2IterationCount = value;
					NotifyPropertyChanged( "Phase2IterationCount" );
				}
			}
		}

		public bool MinimizeHierarchicalEdgeLong
		{
			get { return _minimizeHierarchicalEdgeLong; }
			set
			{
				if ( _minimizeHierarchicalEdgeLong != value )
				{
					_minimizeHierarchicalEdgeLong = value;
					NotifyPropertyChanged( "MinimizeHierarchicalEdgeLong" );
				}
			}
		}

		public PositionCalculationMethodTypes PositionCalculationMethod
		{
			get { return _positionCalculationMethod; }
			set
			{
				if ( value != _positionCalculationMethod )
				{
					_positionCalculationMethod = value;
					NotifyPropertyChanged( "PositionCalculationMethod" );
				}
			}
		}

		/// <summary>
		/// Gets or sets the 'Simplify' parameter.
		/// If true than the edges which directly goes to a vertex which could 
		/// be reached on another path (which is not directly goes to that vertex, there's some plus vertices)
		/// will not be count in the layout algorithm.
		/// </summary>
		public bool Simplify
		{
			get { return _simplify; }
			set
			{
				if ( _simplify != value )
				{
					_simplify = value;
					NotifyPropertyChanged( "Simplify" );
				}
			}
		}

		/// <summary>
		/// Prompting constraint type for the starting positions.
		/// </summary>
		public PromptingConstraintType Prompting
		{
			get { return _promptingConstraint; }
			set
			{
				if ( _promptingConstraint != value )
				{
					_promptingConstraint = value;
					NotifyPropertyChanged( "Prompting" );
				}
			}
		}
	}
}