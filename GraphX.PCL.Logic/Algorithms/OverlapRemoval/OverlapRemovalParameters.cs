using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Logic.Algorithms.OverlapRemoval
{
	public class OverlapRemovalParameters : IOverlapRemovalParameters
	{
		private float _verticalGap = 10;
		private float _horizontalGap = 10;

        /// <summary>
        /// Gets or sets minimal vertical distance between vertices
        /// </summary>
		public float VerticalGap
		{
			get { return _verticalGap; }
			set
			{
			    if (_verticalGap == value) return;
			    _verticalGap = value;
			    NotifyChanged( "VerticalGap" );
			}
		}

        /// <summary>
        /// Gets or sets minimal horizontal distance between vertices
        /// </summary>
		public float HorizontalGap
		{
			get { return _horizontalGap; }
			set
			{
			    if (_horizontalGap == value) return;
			    _horizontalGap = value;
			    NotifyChanged( "HorizontalGap" );
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		protected void NotifyChanged( string propertyName )
		{
			if ( PropertyChanged != null )
				PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
		}

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
	}
}