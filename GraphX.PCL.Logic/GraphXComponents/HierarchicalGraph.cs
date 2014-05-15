using System.Collections.Generic;
using QuickGraph;
using System.Linq;

namespace GraphX.GraphSharp
{
	public class HierarchicalGraph<TVertex, TEdge> :
		BidirectionalGraph<TVertex, TEdge>, IHierarchicalBidirectionalGraph<TVertex, TEdge>
		where TEdge : TypedEdge<TVertex>
	{
		#region Properties, fields
		private class TypedEdgeCollectionWrapper
		{
			public readonly List<TEdge> InHierarchicalEdges = new List<TEdge>();
			public readonly List<TEdge> OutHierarchicalEdges = new List<TEdge>();
			public readonly List<TEdge> InGeneralEdges = new List<TEdge>();
			public readonly List<TEdge> OutGeneralEdges = new List<TEdge>();
		}
		private readonly Dictionary<TVertex, TypedEdgeCollectionWrapper> typedEdgeCollections = new Dictionary<TVertex, TypedEdgeCollectionWrapper>();
		#endregion

		#region Constructors
		public HierarchicalGraph()
		{ }

		public HierarchicalGraph(bool allowParallelEdges)
			: base(allowParallelEdges) { }

		public HierarchicalGraph(bool allowParallelEdges, int vertexCapacity)
			: base(allowParallelEdges, vertexCapacity) { }
		#endregion

		#region Add/Remove Vertex
		public override bool AddVertex(TVertex v)
		{
			base.AddVertex(v);
			if (!typedEdgeCollections.ContainsKey(v))
			{
				typedEdgeCollections[v] = new TypedEdgeCollectionWrapper();
			}
			return true;
		}

		public override bool RemoveVertex(TVertex v)
		{
			bool ret = base.RemoveVertex(v);
			if (ret)
			{
				//remove the edges from the typedEdgeCollections
				TypedEdgeCollectionWrapper edgeCollection = typedEdgeCollections[v];
				foreach (TEdge e in edgeCollection.InGeneralEdges)
					typedEdgeCollections[e.Source].OutGeneralEdges.Remove(e);
				foreach (TEdge e in edgeCollection.OutGeneralEdges)
					typedEdgeCollections[e.Target].InGeneralEdges.Remove(e);

				foreach (TEdge e in edgeCollection.InHierarchicalEdges)
					typedEdgeCollections[e.Source].OutHierarchicalEdges.Remove(e);
				foreach (TEdge e in edgeCollection.OutHierarchicalEdges)
					typedEdgeCollections[e.Target].InHierarchicalEdges.Remove(e);

				typedEdgeCollections.Remove(v);
				return true;
			}
			
			return false;
		}
		#endregion

		#region Add/Remove Edge
		public override bool AddEdge(TEdge e)
		{
			if (base.AddEdge(e))
			{
				//add edge to the source collections
				TypedEdgeCollectionWrapper sourceEdgeCollection = typedEdgeCollections[e.Source];
				switch (e.Type)
				{
					case EdgeTypes.General:
						sourceEdgeCollection.OutGeneralEdges.Add(e);
						break;
					case EdgeTypes.Hierarchical:
						sourceEdgeCollection.OutHierarchicalEdges.Add(e);
						break;
					default:
						break;
				}

				//add edge to the target collections
				TypedEdgeCollectionWrapper targetEdgeCollection = typedEdgeCollections[e.Target];
				switch (e.Type)
				{
					case EdgeTypes.General:
						targetEdgeCollection.InGeneralEdges.Add(e);
						break;
					case EdgeTypes.Hierarchical:
						targetEdgeCollection.InHierarchicalEdges.Add(e);
						break;
					default:
						break;
				}
				return true;
			}
			return false;
		}

		public override bool RemoveEdge(TEdge e)
		{
			if (base.RemoveEdge(e))
			{
				//remove edge from the source collections
				TypedEdgeCollectionWrapper sourceEdgeCollection = typedEdgeCollections[e.Source];
				switch (e.Type)
				{
					case EdgeTypes.General:
						sourceEdgeCollection.OutGeneralEdges.Remove(e);
						break;
					case EdgeTypes.Hierarchical:
						sourceEdgeCollection.OutHierarchicalEdges.Remove(e);
						break;
					default:
						break;
				}

				//remove edge from the target collections
				TypedEdgeCollectionWrapper targetEdgeCollection = typedEdgeCollections[e.Target];
				switch (e.Type)
				{
					case EdgeTypes.General:
						targetEdgeCollection.InGeneralEdges.Remove(e);
						break;
					case EdgeTypes.Hierarchical:
						targetEdgeCollection.InHierarchicalEdges.Remove(e);
						break;
					default:
						break;
				}
				return true;
			}
			return false;
		}

		#endregion

		#region Hierarchical Edges
		public IEnumerable<TEdge> HierarchicalEdgesFor(TVertex v)
		{
			TypedEdgeCollectionWrapper collections = typedEdgeCollections[v];
			return collections.InHierarchicalEdges.Concat(collections.OutHierarchicalEdges);
		}

		public int HierarchicalEdgeCountFor(TVertex v)
		{
			return typedEdgeCollections[v].InHierarchicalEdges.Count + typedEdgeCollections[v].OutHierarchicalEdges.Count;
		}

		public IEnumerable<TEdge> InHierarchicalEdges(TVertex v)
		{
			return typedEdgeCollections[v].InHierarchicalEdges;
		}

		public int InHierarchicalEdgeCount(TVertex v)
		{
			return typedEdgeCollections[v].InHierarchicalEdges.Count;
		}

		public IEnumerable<TEdge> OutHierarchicalEdges(TVertex v)
		{
			return typedEdgeCollections[v].OutHierarchicalEdges;
		}

		public int OutHierarchicalEdgeCount(TVertex v)
		{
			return typedEdgeCollections[v].OutHierarchicalEdges.Count;
		}
		#endregion

		#region General Edges
		public IEnumerable<TEdge> GeneralEdgesFor(TVertex v)
		{
			TypedEdgeCollectionWrapper collections = typedEdgeCollections[v];
			foreach (TEdge e in collections.InGeneralEdges)
			{
				yield return e;
			}
			foreach (TEdge e in collections.OutGeneralEdges)
			{
				yield return e;
			}
		}

		public int GeneralEdgeCountFor(TVertex v)
		{
			return typedEdgeCollections[v].InGeneralEdges.Count + typedEdgeCollections[v].OutGeneralEdges.Count;
		}

		public IEnumerable<TEdge> InGeneralEdges(TVertex v)
		{
			return typedEdgeCollections[v].InGeneralEdges;
		}

		public int InGeneralEdgeCount(TVertex v)
		{
			return typedEdgeCollections[v].InGeneralEdges.Count;
		}

		public IEnumerable<TEdge> OutGeneralEdges(TVertex v)
		{
			return typedEdgeCollections[v].OutGeneralEdges;
		}

		public int OutGeneralEdgeCount(TVertex v)
		{
			return typedEdgeCollections[v].OutGeneralEdges.Count;
		}
		#endregion

		#region IHierarchicalBidirectionalGraph<TVertex,TEdge> Members


		public IEnumerable<TEdge> HierarchicalEdges
		{
			get
			{
				foreach (TVertex v in Vertices)
				{
					foreach (TEdge e in OutHierarchicalEdges(v))
					{
						yield return e;
					}
				}
			}
		}

		public int HierarchicalEdgeCount
		{
			get
			{
				return Vertices.Sum(v => InHierarchicalEdgeCount(v));
			}
		}

		public IEnumerable<TEdge> GeneralEdges
		{
			get
			{
				foreach (TVertex v in Vertices)
				{
					foreach (TEdge e in OutGeneralEdges(v))
					{
						yield return e;                        
					}
				}
			}
		}

		public int GeneralEdgeCount
		{
			get
			{
				return Vertices.Sum(v => InGeneralEdgeCount(v));
			}
		}

		#endregion
	}
}