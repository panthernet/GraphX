namespace GraphX.PCL.Logic.Algorithms.OverlapRemoval
{
	public enum OneWayFSAWayEnum {
		Horizontal,
		Vertical
	}

	public class OneWayFSAParameters : OverlapRemovalParameters
	{
		private OneWayFSAWayEnum _way;
		public OneWayFSAWayEnum Way
		{
			get { return _way; }
			set
			{
				if ( _way != value )
				{
					_way = value;
					NotifyChanged( "Way" );
				}
			}
		}
	}
}