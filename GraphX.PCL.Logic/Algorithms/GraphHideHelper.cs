using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms
{
	internal class GraphHideHelper<TVertex, TEdge> : ISoftMutableGraph<TVertex, TEdge>
		where TEdge : IEdge<TVertex>
	{
		private readonly IMutableBidirectionalGraph<TVertex, TEdge> _graph;

		#region Helper Types
		protected class HiddenCollection
		{
			public List<TVertex> hiddenVertices = new List<TVertex>();
			public List<TEdge> hiddenEdges = new List<TEdge>();
		}
		#endregion

		#region Properties, fields, events
		private readonly List<TVertex> _hiddenVertices = new List<TVertex>();
		private readonly List<TEdge> _hiddenEdges = new List<TEdge>();
		private readonly IDictionary<string, HiddenCollection> _hiddenCollections = new Dictionary<string, HiddenCollection>();
		private readonly IDictionary<TVertex, List<TEdge>> _hiddenEdgesOf = new Dictionary<TVertex, List<TEdge>>();

		public event EdgeAction<TVertex, TEdge> EdgeHidden;
		public event EdgeAction<TVertex, TEdge> EdgeUnhidden;

		public event VertexAction<TVertex> VertexHidden;
		public event VertexAction<TVertex> VertexUnhidden;
		#endregion

		public GraphHideHelper( IMutableBidirectionalGraph<TVertex, TEdge> managedGraph )
		{
			_graph = managedGraph;
		}

		#region Event handlers, helper methods

		/// <summary>
		/// Returns every edge connected with the vertex <code>v</code>.
		/// </summary>
		/// <param name="v">The vertex.</param>
		/// <returns>Edges, adjacent to the vertex <code>v</code>.</returns>
		protected IEnumerable<TEdge> EdgesFor( TVertex v )
		{
			return _graph.InEdges( v ).Concat( _graph.OutEdges( v ) );
		}

		protected HiddenCollection GetHiddenCollection( string tag )
		{
			HiddenCollection h;
			if ( !_hiddenCollections.TryGetValue( tag, out h ) )
			{
				h = new HiddenCollection();
				_hiddenCollections[tag] = h;
			}
			return h;
		}

		protected void OnEdgeHidden( TEdge e )
		{
			if ( EdgeHidden != null )
				EdgeHidden( e );
		}

		protected void OnEdgeUnhidden( TEdge e )
		{
			if ( EdgeUnhidden != null )
				EdgeUnhidden( e );
		}

		protected void OnVertexHidden( TVertex v )
		{
			if ( VertexHidden != null )
				VertexHidden( v );
		}

		protected void OnVertexUnhidden( TVertex v )
		{
			if ( VertexUnhidden != null )
				VertexUnhidden( v );
		}
		#endregion

		#region ISoftMutableGraph<TVertex,TEdge> Members

		public IEnumerable<TVertex> HiddenVertices
		{
			get { return _hiddenVertices; }
		}

		public IEnumerable<TEdge> HiddenEdges
		{
			get { return _hiddenEdges; }
		}

		/// <summary>
		/// Hides the vertex <code>v</code>.
		/// </summary>
		/// <param name="v">The vertex to hide.</param>
		public bool HideVertex( TVertex v )
		{
			if ( _graph.ContainsVertex( v ) && !_hiddenVertices.Contains( v ) )
			{
				HideEdges( EdgesFor( v ) );

				//hide the vertex
				_graph.RemoveVertex( v );
				_hiddenVertices.Add( v );
				OnVertexHidden( v );
				return true;
			}

			return false;
		}

		/// <summary>
		/// Hides a lot of vertices.
		/// </summary>
		/// <param name="vertices">The vertices to hide.</param>
		public void HideVertices( IEnumerable<TVertex> vertices )
		{
			var verticesToHide = new List<TVertex>( vertices );
			foreach ( TVertex v in verticesToHide )
			{
				HideVertex( v );
			}
		}

		public bool HideVertex( TVertex v, string tag )
		{
			HiddenCollection h = GetHiddenCollection( tag );
			var eeh = new EdgeAction<TVertex, TEdge>( e => h.hiddenEdges.Add( e ) );
			var veh = new VertexAction<TVertex>( vertex => h.hiddenVertices.Add( vertex ) );
			EdgeHidden += eeh;
			VertexHidden += veh;
			bool ret = HideVertex( v );
			EdgeHidden -= eeh;
			VertexHidden -= veh;
			return ret;
		}

		public void HideVertices( IEnumerable<TVertex> vertices, string tag )
		{
			foreach ( var v in vertices )
			{
				HideVertex( v, tag );
			}
		}

		public void HideVerticesIf( Func<TVertex, bool> predicate, string tag )
		{
			var verticesToHide = _graph.Vertices.Where(predicate).ToList();
		    HideVertices( verticesToHide, tag );
		}

		public bool IsHiddenVertex( TVertex v )
		{
			return ( !_graph.ContainsVertex( v ) && _hiddenVertices.Contains( v ) );
		}

		public bool UnhideVertex( TVertex v )
		{
			//if v not hidden, it's an error
			if ( !IsHiddenVertex( v ) )
				return false;

			//unhide the vertex
			_graph.AddVertex( v );
			_hiddenVertices.Remove( v );
			OnVertexUnhidden( v );
			return true;
		}

		public void UnhideVertexAndEdges( TVertex v )
		{
			UnhideVertex( v );
			List<TEdge> hiddenEdgesList;
			_hiddenEdgesOf.TryGetValue( v, out hiddenEdgesList );
			if ( hiddenEdgesList != null )
				UnhideEdges( hiddenEdgesList );
		}

		public bool HideEdge( TEdge e )
		{
			if ( _graph.ContainsEdge( e ) && !_hiddenEdges.Contains( e ) )
			{
				_graph.RemoveEdge( e );
				_hiddenEdges.Add( e );

				GetHiddenEdgeListOf( e.Source ).Add( e );
				GetHiddenEdgeListOf( e.Target ).Add( e );

				OnEdgeHidden( e );
				return true;
			}

			return false;
		}

		private List<TEdge> GetHiddenEdgeListOf( TVertex v )
		{
			List<TEdge> hiddenEdgeList;
			_hiddenEdgesOf.TryGetValue( v, out hiddenEdgeList );
			if ( hiddenEdgeList == null )
			{
				hiddenEdgeList = new List<TEdge>();
				_hiddenEdgesOf[v] = hiddenEdgeList;
			}
			return hiddenEdgeList;
		}

		public IEnumerable<TEdge> HiddenEdgesOf( TVertex v )
		{
			return GetHiddenEdgeListOf( v );
		}

		public int HiddenEdgeCountOf( TVertex v )
		{
			return GetHiddenEdgeListOf( v ).Count;
		}

		public bool HideEdge( TEdge e, string tag )
		{
			var h = GetHiddenCollection( tag );
			var eeh = new EdgeAction<TVertex, TEdge>( edge => h.hiddenEdges.Add( edge ) );
			EdgeHidden += eeh;
			bool ret = HideEdge( e );
			EdgeHidden -= eeh;
			return ret;
		}

		public void HideEdges( IEnumerable<TEdge> edges )
		{
			var edgesToHide = new List<TEdge>( edges );
			foreach ( var e in edgesToHide )
			{
				HideEdge( e );
			}
		}

		public void HideEdges( IEnumerable<TEdge> edges, string tag )
		{
			var edgesToHide = new List<TEdge>( edges );
			foreach ( var e in edgesToHide )
			{
				HideEdge( e, tag );
			}
		}

		public void HideEdgesIf( Func<TEdge, bool> predicate, string tag )
		{
			var edgesToHide = _graph.Edges.Where(predicate).ToList();
		    HideEdges( edgesToHide, tag );
		}

		public bool IsHiddenEdge( TEdge e )
		{
			return ( !_graph.ContainsEdge( e ) && _hiddenEdges.Contains( e ) );
		}

		public bool UnhideEdge( TEdge e )
		{
			if ( IsHiddenVertex( e.Source ) || IsHiddenVertex( e.Target ) || !IsHiddenEdge( e ) )
				return false;

			//unhide the edge
			_graph.AddEdge( e );
			_hiddenEdges.Remove( e );

			GetHiddenEdgeListOf( e.Source ).Remove( e );
			GetHiddenEdgeListOf( e.Target ).Remove( e );

			OnEdgeUnhidden( e );
			return true;
		}

		public void UnhideEdgesIf( Func<TEdge, bool> predicate )
		{
			var edgesToUnhide = _hiddenEdges.Where(predicate).ToList();
		    UnhideEdges( edgesToUnhide );
		}

		public void UnhideEdges( IEnumerable<TEdge> edges )
		{
			var edgesToUnhide = new List<TEdge>( edges );
			foreach ( var e in edgesToUnhide )
			{
				UnhideEdge( e );
			}
		}

		public bool Unhide( string tag )
		{
			HiddenCollection h = GetHiddenCollection( tag );
			foreach ( TVertex v in h.hiddenVertices )
			{
				UnhideVertex( v );
			}
			foreach ( TEdge e in h.hiddenEdges )
			{
				UnhideEdge( e );
			}
			return _hiddenCollections.Remove( tag );
		}

		public bool UnhideAll()
		{
			while ( _hiddenVertices.Count > 0 )
			{
				UnhideVertex( _hiddenVertices[0] );
			}
			while ( _hiddenEdges.Count > 0 )
			{
				UnhideEdge( _hiddenEdges[0] );
			}
			return true;
		}

		public int HiddenVertexCount
		{
			get { return _hiddenVertices.Count; }
		}

		public int HiddenEdgeCount
		{
			get { return _hiddenEdges.Count; }
		}
		#endregion

		#region IBidirectionalGraph<TVertex,TEdge> Members

		public int Degree( TVertex v )
		{
			throw new NotImplementedException();
		}

		public int InDegree( TVertex v )
		{
			throw new NotImplementedException();
		}

		public TEdge InEdge( TVertex v, int index )
		{
			throw new NotImplementedException();
		}

		public IEnumerable<TEdge> InEdges( TVertex v )
		{
			throw new NotImplementedException();
		}

		public bool IsInEdgesEmpty( TVertex v )
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IIncidenceGraph<TVertex,TEdge> Members

		public bool ContainsEdge( TVertex source, TVertex target )
		{
			throw new NotImplementedException();
		}

		public bool TryGetEdge( TVertex source, TVertex target, out TEdge edge )
		{
			throw new NotImplementedException();
		}

		public bool TryGetEdges( TVertex source, TVertex target, out IEnumerable<TEdge> edges )
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IImplicitGraph<TVertex,TEdge> Members

		public bool IsOutEdgesEmpty( TVertex v )
		{
			throw new NotImplementedException();
		}

		public int OutDegree( TVertex v )
		{
			throw new NotImplementedException();
		}

		public TEdge OutEdge( TVertex v, int index )
		{
			throw new NotImplementedException();
		}

		public IEnumerable<TEdge> OutEdges( TVertex v )
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IGraph<TVertex,TEdge> Members

		public bool AllowParallelEdges
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsDirected
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region IVertexSet<TVertex,TEdge> Members

		public bool ContainsVertex( TVertex vertex )
		{
			throw new NotImplementedException();
		}

		public bool IsVerticesEmpty
		{
			get { throw new NotImplementedException(); }
		}

		public int VertexCount
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<TVertex> Vertices
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region IEdgeListGraph<TVertex,TEdge> Members

		public bool ContainsEdge( TEdge edge )
		{
			throw new NotImplementedException();
		}

		public int EdgeCount
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<TEdge> Edges
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsEdgesEmpty
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		public bool TryGetInEdges( TVertex v, out IEnumerable<TEdge> edges )
		{
			throw new NotImplementedException();
		}
		
		public bool TryGetOutEdges( TVertex v, out IEnumerable<TEdge> edges )
		{
			throw new NotImplementedException();
		}
	}
}