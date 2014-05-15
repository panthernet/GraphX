namespace GraphX.GraphSharp
{
	public class WrappedVertex<TVertex>
	{
		private TVertex originalVertex;
		public TVertex Original
		{
			get { return originalVertex; }
		}

		public WrappedVertex(TVertex original)
		{
			this.originalVertex = original;
		}
	}
}