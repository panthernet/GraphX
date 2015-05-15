namespace GraphX.PCL.Common.Interfaces
{
	public interface IOverlapRemovalParameters : IAlgorithmParameters
	{
		float VerticalGap { get; set; }
		float HorizontalGap { get; set; }
	}
}