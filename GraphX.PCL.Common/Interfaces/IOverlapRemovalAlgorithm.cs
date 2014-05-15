namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
    public interface IOverlapRemovalAlgorithm<TObject> : IExternalOverlapRemoval<TObject>
	{
		IOverlapRemovalParameters GetParameters();
	}

	public interface IOverlapRemovalAlgorithm<TObject, TParam> : IOverlapRemovalAlgorithm<TObject>
		where TParam : IOverlapRemovalParameters
	{
		TParam Parameters { get; }
	}
}