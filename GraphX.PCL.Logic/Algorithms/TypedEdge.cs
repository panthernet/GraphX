using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms
{
	public enum EdgeTypes
	{
		General,
		Hierarchical
	}

	public interface ITypedEdge
	{
		EdgeTypes Type { get; }
	}

	public class TypedEdge<TVertex> : Edge<TVertex>, ITypedEdge
	{
		private readonly EdgeTypes _type;
		public EdgeTypes Type
		{
			get { return _type; }
		}

		public TypedEdge(TVertex source, TVertex target, EdgeTypes type)
			: base(source, target)
		{
			_type = type;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}-->{2}", _type, Source, Target);
		}
	}
}