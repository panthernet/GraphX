using System;
using System.Collections.Generic;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms
{
	/// <summary>
	/// Soft mutation means: We can hide the vertices or the edges.
	/// </summary>
	/// <typeparam name="TVertex"></typeparam>
	/// <typeparam name="TEdge"></typeparam>
	public interface ISoftMutableGraph<TVertex, TEdge> : IBidirectionalGraph<TVertex, TEdge>
		where TEdge : IEdge<TVertex>
	{
		bool HideVertex( TVertex v );
		bool HideVertex( TVertex v, string tag );
		void HideVertices( IEnumerable<TVertex> vertices );
		void HideVertices( IEnumerable<TVertex> vertices, string tag );
		void HideVerticesIf( Func<TVertex, bool> predicate, string tag );
		bool IsHiddenVertex( TVertex v );
		bool UnhideVertex( TVertex v );
		void UnhideVertexAndEdges( TVertex v );
		IEnumerable<TVertex> HiddenVertices { get; }
		int HiddenVertexCount { get; }

		bool HideEdge( TEdge e );
		bool HideEdge( TEdge e, string tag );
		void HideEdges( IEnumerable<TEdge> edges );
		void HideEdges( IEnumerable<TEdge> edges, string tag );
		void HideEdgesIf( Func<TEdge, bool> predicate, string tag );
		bool IsHiddenEdge( TEdge e );
		bool UnhideEdge( TEdge e );
		void UnhideEdges( IEnumerable<TEdge> edges );
		void UnhideEdgesIf( Func<TEdge, bool> predicate );
		IEnumerable<TEdge> HiddenEdgesOf( TVertex v );
		int HiddenEdgeCountOf( TVertex v );
		IEnumerable<TEdge> HiddenEdges { get; }
		int HiddenEdgeCount { get; }

		bool Unhide( string tag );
		bool UnhideAll( );
	}
}