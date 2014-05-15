namespace GraphX.GraphSharp.Algorithms.EdgeRouting
{
	public class EdgeRoutingParameters : IEdgeRoutingParameters
	{
		public object Clone()
		{
			return this.MemberwiseClone();
		}

		protected void NotifyChanged( string propertyName )
		{
			if ( PropertyChanged != null )
				PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
		}

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
	}
}