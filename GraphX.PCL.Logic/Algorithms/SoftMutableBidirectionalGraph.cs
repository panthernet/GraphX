using System;
using System.Collections.Generic;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms
{
	public class SoftMutableBidirectionalGraph<TVertex, TEdge> : BidirectionalGraph<TVertex, TEdge>, ISoftMutableGraph<TVertex, TEdge>
		where TEdge : IEdge<TVertex>
	{
		private GraphHideHelper<TVertex, TEdge> _hideHelper;

		#region Events
		public event EdgeAction<TVertex, TEdge> EdgeHidden
		{
			add { _hideHelper.EdgeHidden += value;}
			remove { _hideHelper.EdgeHidden -= value;}
		}
		public event EdgeAction<TVertex, TEdge> EdgeUnhidden
		{
			add { _hideHelper.EdgeUnhidden += value;}
			remove { _hideHelper.EdgeUnhidden -= value;}
		}

		public event VertexAction<TVertex> VertexHidden
		{
			add { _hideHelper.VertexHidden += value;}
			remove { _hideHelper.VertexHidden -= value;}
		}
		public event VertexAction<TVertex> VertexUnhidden
		{
			add { _hideHelper.VertexUnhidden += value;}
			remove { _hideHelper.VertexUnhidden -= value;}
		}
		#endregion

		protected void Init()
		{
			_hideHelper = new GraphHideHelper<TVertex, TEdge>( this );
		}

		public SoftMutableBidirectionalGraph()
		{
			Init();
		}

		public SoftMutableBidirectionalGraph( bool allowParallelEdges )
			: base( allowParallelEdges )
		{
			Init();
		}

		public SoftMutableBidirectionalGraph( bool allowParallelEdges, int vertexCapacity)
			: base( allowParallelEdges, vertexCapacity )
		{
			Init();
		}

		//Delegating the calls to the GraphHideHelper helper class

		#region ISoftMutableGraph<TVertex,TEdge> Members

		public bool HideVertex( TVertex v )
		{
			return _hideHelper.HideVertex( v );
		}

		public bool HideVertex( TVertex v, string tag )
		{
			return _hideHelper.HideVertex( v, tag );
		}

		public void HideVertices( IEnumerable<TVertex> vertices )
		{
			_hideHelper.HideVertices( vertices );
		}

		public void HideVertices( IEnumerable<TVertex> vertices, string tag )
		{
			_hideHelper.HideVertices( vertices, tag );
		}

		public void HideVerticesIf( Func<TVertex, bool> predicate, string tag )
		{
			_hideHelper.HideVerticesIf( predicate, tag );
		}

		public bool IsHiddenVertex( TVertex v )
		{
			return _hideHelper.IsHiddenVertex( v );
		}

		public bool UnhideVertex( TVertex v )
		{
			return _hideHelper.UnhideVertex( v );
		}

		public void UnhideVertexAndEdges( TVertex v )
		{
			_hideHelper.UnhideVertexAndEdges( v );
		}

		public IEnumerable<TVertex> HiddenVertices
		{
			get { return _hideHelper.HiddenVertices; }
		}

		public int HiddenVertexCount
		{
			get { return _hideHelper.HiddenVertexCount; }
		}

		public bool HideEdge( TEdge e )
		{
			return _hideHelper.HideEdge( e );
		}

		public bool HideEdge( TEdge e, string tag )
		{
			return _hideHelper.HideEdge( e, tag );
		}

		public void HideEdges( IEnumerable<TEdge> edges )
		{
			_hideHelper.HideEdges( edges );
		}

		public void HideEdges( IEnumerable<TEdge> edges, string tag )
		{
			_hideHelper.HideEdges( edges, tag );
		}

		public void HideEdgesIf( Func<TEdge, bool> predicate, string tag )
		{
			_hideHelper.HideEdgesIf( predicate, tag );
		}

		public bool IsHiddenEdge( TEdge e )
		{
			return _hideHelper.IsHiddenEdge( e );
		}

		public bool UnhideEdge( TEdge e )
		{
			return _hideHelper.UnhideEdge( e );
		}

		public void UnhideEdges( IEnumerable<TEdge> edges )
		{
			_hideHelper.UnhideEdges( edges );
		}

		public void UnhideEdgesIf( Func<TEdge, bool> predicate )
		{
			_hideHelper.UnhideEdgesIf( predicate );
		}

		public IEnumerable<TEdge> HiddenEdgesOf( TVertex v )
		{
			return _hideHelper.HiddenEdgesOf( v );
		}

		public int HiddenEdgeCountOf( TVertex v )
		{
			return _hideHelper.HiddenEdgeCountOf( v );
		}

		public IEnumerable<TEdge> HiddenEdges
		{
			get { return _hideHelper.HiddenEdges; }
		}

		public int HiddenEdgeCount
		{
			get { return _hideHelper.HiddenEdgeCount; }
		}

		public bool Unhide( string tag )
		{
			return _hideHelper.Unhide( tag );
		}

		public bool UnhideAll()
		{
			return _hideHelper.UnhideAll();
		}

		#endregion
	}
}