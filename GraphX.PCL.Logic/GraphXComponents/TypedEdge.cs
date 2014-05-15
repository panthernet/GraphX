using QuickGraph;

namespace GraphX.GraphSharp
{
	public enum EdgeTypes
	{
		General,
		Hierarchical
	}

	public interface ITypedEdge<TVertex>
	{
		EdgeTypes Type { get; }
	}

	public class TypedEdge<TVertex> : Edge<TVertex>, ITypedEdge<TVertex>
	{
		private EdgeTypes type;
		public EdgeTypes Type
		{
			get { return this.type; }
		}

		public TypedEdge(TVertex source, TVertex target, EdgeTypes type)
			: base(source, target)
		{
			this.type = type;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}-->{2}", type, this.Source, this.Target);
		}
	}
}