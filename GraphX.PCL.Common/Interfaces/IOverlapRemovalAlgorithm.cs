namespace GraphX.PCL.Common.Interfaces
{
    public interface IOverlapRemovalAlgorithm<TObject> : IExternalOverlapRemoval<TObject>
	{
		IOverlapRemovalParameters GetParameters();
	}

	public interface IOverlapRemovalAlgorithm<TObject, out TParam> : IOverlapRemovalAlgorithm<TObject>
		where TParam : IOverlapRemovalParameters
	{
		TParam Parameters { get; }
	}
}