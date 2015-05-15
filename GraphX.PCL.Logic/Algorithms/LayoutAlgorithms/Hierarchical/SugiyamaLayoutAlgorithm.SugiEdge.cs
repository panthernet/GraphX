using System.Collections.Generic;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public partial class SugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph> 
        where TVertex : class 
        where TEdge : IEdge<TVertex> 
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
		private class SugiEdge : TypedEdge<SugiVertex>
		{
			public bool IsLongEdge
			{
				get { return DummyVertices != null; }
				set
				{
					if ( IsLongEdge != value )
					{
						DummyVertices = value ? new List<SugiVertex>() : null;
					}
				}
			}

			public IList<SugiVertex> DummyVertices { get; private set; }
			public TEdge Original { get; private set; }
			public bool IsReverted
			{
				get
				{
					return !Original.Equals(default(TEdge)) && Original.Source == Target.Original && Original.Target == Source.Original;
				}
			}

			public SugiEdge( TEdge original, SugiVertex source, SugiVertex target, EdgeTypes type )
				: base( source, target, type )
			{
				Original = original;
			}
		}
	}
}