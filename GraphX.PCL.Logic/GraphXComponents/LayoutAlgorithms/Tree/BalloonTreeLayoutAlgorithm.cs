using System;
using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.Tree
{
	public class BalloonTreeLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, BalloonTreeLayoutParameters>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
		protected readonly TVertex root;
		private readonly IDictionary<TVertex, Size> vertexSizes;
		private readonly IDictionary<TVertex, BalloonData> datas = new Dictionary<TVertex, BalloonData>();
		private HashSet<TVertex> visitedVertices = new HashSet<TVertex>();

		private class BalloonData
		{
			public int d;
			public int r;
			public float a;
			public float c;
			public float f;
		}


		public BalloonTreeLayoutAlgorithm(
			TGraph visitedGraph,
			IDictionary<TVertex, Point> vertexPositions,
			IDictionary<TVertex, Size> vertexSizes,
			BalloonTreeLayoutParameters oldParameters,
			TVertex selectedVertex )
			: base( visitedGraph, vertexPositions, oldParameters )
		{
			this.root = selectedVertex;
			this.vertexSizes = vertexSizes;
		}

		protected override void InternalCompute()
		{
			InitializeData();

			FirstWalk( root );

			visitedVertices.Clear();

			SecondWalk( root, null, 0, 0, 1, 0 );

			NormalizePositions();
		}

		private void FirstWalk( TVertex v )
		{
			var data = datas[v];
			visitedVertices.Add( v );
			data.d = 0;

			float s = 0;

			foreach ( var edge in VisitedGraph.OutEdges( v ) )
			{
				var otherVertex = edge.Target;
				var otherData = datas[otherVertex];

				if ( !visitedVertices.Contains( otherVertex ) )
				{
					FirstWalk( otherVertex );
					data.d = Math.Max( data.d, otherData.r );
					otherData.a = (float)Math.Atan( ( (float)otherData.r ) / ( data.d + otherData.r ) );
					s += otherData.a;
				}
			}


			AdjustChildren( v, data, s );
			SetRadius( v, data );
		}

		private void SecondWalk( TVertex v, TVertex r, double x, double y, float l, float t )
		{
			Point pos = new Point( x, y );
			VertexPositions[v] = pos;
			visitedVertices.Add( v );
			BalloonData data = datas[v];

			float dd = l * data.d;
			float p = (float)( t + Math.PI );
			int degree = VisitedGraph.OutDegree( v );
			float fs = ( degree == 0 ? 0 : data.f / degree );
			float pr = 0;

			foreach ( var edge in VisitedGraph.OutEdges( v ) )
			{
				var otherVertex = edge.Target;
				if ( visitedVertices.Contains( otherVertex ) )
					continue;

				var otherData = datas[otherVertex];
				float aa = data.c * otherData.a;
				float rr = (float)( data.d * Math.Tan( aa ) / ( 1 - Math.Tan( aa ) ) );
				p += pr + aa + fs;

				float xx = (float)( ( l * rr + dd ) * Math.Cos( p ) );
				float yy = (float)( ( l * rr + dd ) * Math.Sign( p ) );
				pr = aa; ;
				SecondWalk( otherVertex, v, x + xx, y + yy, l * data.c, p );
			}
		}

		private void SetRadius( TVertex v, BalloonData data )
		{
			data.r = (int)Math.Max( data.d / 2, Parameters.minRadius );
		}

		private void AdjustChildren( TVertex v, BalloonData data, float s )
		{
			if ( s > Math.PI )
			{
				data.c = (float)Math.PI / s;
				data.f = 0;
			}
			else
			{
				data.c = 1;
				data.f = (float)Math.PI - s;
			}
		}

		private void InitializeData()
		{
			foreach ( var v in VisitedGraph.Vertices )
				datas[v] = new BalloonData();

			visitedVertices.Clear();
		}
	}
}
