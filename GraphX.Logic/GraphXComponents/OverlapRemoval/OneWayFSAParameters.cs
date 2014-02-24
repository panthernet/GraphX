namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
	public enum OneWayFSAWayEnum {
		Horizontal,
		Vertical
	}

	public class OneWayFSAParameters : OverlapRemovalParameters
	{
		private OneWayFSAWayEnum way;
		public OneWayFSAWayEnum Way
		{
			get { return way; }
			set
			{
				if ( way != value )
				{
					way = value;
					NotifyChanged( "Way" );
				}
			}
		}
	}
}