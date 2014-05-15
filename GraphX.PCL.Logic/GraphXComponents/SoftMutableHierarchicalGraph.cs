using System;
using System.Collections.Generic;
using QuickGraph;

namespace GraphX.GraphSharp
{
	public class SoftMutableHierarchicalGraph<TVertex, TEdge> : HierarchicalGraph<TVertex, TEdge>, ISoftMutableGraph<TVertex, TEdge>
		where TEdge : TypedEdge<TVertex>
	{
		private GraphHideHelper<TVertex, TEdge> hideHelper;

		#region Events
		public event EdgeAction<TVertex, TEdge> EdgeHidden
		{
			add { hideHelper.EdgeHidden += value; }
			remove { hideHelper.EdgeHidden -= value; }
		}
		public event EdgeAction<TVertex, TEdge> EdgeUnhidden
		{
			add { hideHelper.EdgeUnhidden += value; }
			remove { hideHelper.EdgeUnhidden -= value; }
		}

		public event VertexAction<TVertex> VertexHidden
		{
			add { hideHelper.VertexHidden += value; }
			remove { hideHelper.VertexHidden -= value; }
		}
		public event VertexAction<TVertex> VertexUnhidden
		{
			add { hideHelper.VertexUnhidden += value; }
			remove { hideHelper.VertexUnhidden -= value; }
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Called from inside the constructor.
		/// </summary>
		protected void Init()
		{
			hideHelper = new GraphHideHelper<TVertex, TEdge>( this );
		}

		public SoftMutableHierarchicalGraph()
		{
			Init();
		}

		public SoftMutableHierarchicalGraph( bool allowParallelEdges )
			: base( allowParallelEdges )
		{
			Init();
		}

		public SoftMutableHierarchicalGraph( bool allowParallelEdges, int vertexCapacity )
			: base( allowParallelEdges, vertexCapacity )
		{
			Init();
		}
		#endregion

		//Delegating the calls to the GraphHideHelper helper class

		#region ISoftMutableGraph<TVertex,TEdge> Members

		public bool HideVertex( TVertex v )
		{
			return hideHelper.HideVertex( v );
		}

		public bool HideVertex( TVertex v, string tag )
		{
			return hideHelper.HideVertex( v, tag );
		}

		public void HideVertices( IEnumerable<TVertex> vertices )
		{
			hideHelper.HideVertices( vertices );
		}

		public void HideVertices( IEnumerable<TVertex> vertices, string tag )
		{
			hideHelper.HideVertices( vertices, tag );
		}

		public void HideVerticesIf( Func<TVertex, bool> predicate, string tag )
		{
			hideHelper.HideVerticesIf( predicate, tag );
		}

		public bool IsHiddenVertex( TVertex v )
		{
			return hideHelper.IsHiddenVertex( v );
		}

		public bool UnhideVertex( TVertex v )
		{
			return hideHelper.UnhideVertex( v );
		}

		public void UnhideVertexAndEdges( TVertex v )
		{
			hideHelper.UnhideVertexAndEdges( v );
		}

		public IEnumerable<TVertex> HiddenVertices
		{
			get { return hideHelper.HiddenVertices; }
		}

		public int HiddenVertexCount
		{
			get { return hideHelper.HiddenVertexCount; }
		}

		public bool HideEdge( TEdge e )
		{
			return hideHelper.HideEdge( e );
		}

		public bool HideEdge( TEdge e, string tag )
		{
			return hideHelper.HideEdge( e, tag );
		}

		public void HideEdges( IEnumerable<TEdge> edges )
		{
			hideHelper.HideEdges( edges );
		}

		public void HideEdges( IEnumerable<TEdge> edges, string tag )
		{
			hideHelper.HideEdges( edges, tag );
		}

		public void HideEdgesIf( Func<TEdge, bool> predicate, string tag )
		{
			hideHelper.HideEdgesIf( predicate, tag );
		}

		public bool IsHiddenEdge( TEdge e )
		{
			return hideHelper.IsHiddenEdge( e );
		}

		public bool UnhideEdge( TEdge e )
		{
			return hideHelper.UnhideEdge( e );
		}

		public void UnhideEdges( IEnumerable<TEdge> edges )
		{
			hideHelper.UnhideEdges( edges );
		}

		public void UnhideEdgesIf( Func<TEdge, bool> predicate )
		{
			hideHelper.UnhideEdgesIf( predicate );
		}

		public IEnumerable<TEdge> HiddenEdgesOf( TVertex v )
		{
			return hideHelper.HiddenEdgesOf( v );
		}

		public int HiddenEdgeCountOf( TVertex v )
		{
			return hideHelper.HiddenEdgeCountOf( v );
		}

		public IEnumerable<TEdge> HiddenEdges
		{
			get { return hideHelper.HiddenEdges; }
		}

		public int HiddenEdgeCount
		{
			get { return hideHelper.HiddenEdgeCount; }
		}

		public bool Unhide( string tag )
		{
			return hideHelper.Unhide(tag);
		}

		public bool UnhideAll()
		{
			return hideHelper.UnhideAll();
		}

		#endregion
	}
}