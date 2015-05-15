namespace GraphX.PCL.Logic.Algorithms
{
	public class WrappedVertex<TVertex>
	{
		private readonly TVertex _originalVertex;
		public TVertex Original
		{
			get { return _originalVertex; }
		}

		public WrappedVertex(TVertex original)
		{
			_originalVertex = original;
		}
	}
}