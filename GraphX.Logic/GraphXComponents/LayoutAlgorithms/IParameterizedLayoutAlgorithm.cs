namespace GraphX.GraphSharp.Algorithms.Layout
{
	public interface IParameterizedLayoutAlgorithm
	{
		ILayoutParameters GetParameters();
	}

	public interface IParameterizedLayoutAlgorithm<TParam> : IParameterizedLayoutAlgorithm
		where TParam : ILayoutParameters
	{
		TParam Parameters { get; }
	}
}