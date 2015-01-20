namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
	public interface IOverlapRemovalParameters : IAlgorithmParameters
	{
		float VerticalGap { get; set; }
		float HorizontalGap { get; set; }
	}
}