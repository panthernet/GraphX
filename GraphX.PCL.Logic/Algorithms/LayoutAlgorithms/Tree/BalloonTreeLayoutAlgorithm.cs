using System;
using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public class BalloonTreeLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, BalloonTreeLayoutParameters>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
		protected readonly TVertex Root;
	    private readonly IDictionary<TVertex, BalloonData> _datas = new Dictionary<TVertex, BalloonData>();
		private readonly HashSet<TVertex> _visitedVertices = new HashSet<TVertex>();

		private class BalloonData
		{
			public int D;
			public int R;
			public float A;
			public float C;
			public float F;
		}


		public BalloonTreeLayoutAlgorithm(
			TGraph visitedGraph,
			IDictionary<TVertex, Point> vertexPositions,
			BalloonTreeLayoutParameters oldParameters,
			TVertex selectedVertex )
			: base( visitedGraph, vertexPositions, oldParameters )
		{
			Root = selectedVertex;
		}

        public override void Compute(CancellationToken cancellationToken)
		{
			InitializeData();

			FirstWalk( Root );

			_visitedVertices.Clear();

			SecondWalk( Root, null, 0, 0, 1, 0 );

			NormalizePositions();
		}

		private void FirstWalk( TVertex v )
		{
			var data = _datas[v];
			_visitedVertices.Add( v );
			data.D = 0;

			float s = 0;

			foreach ( var edge in VisitedGraph.OutEdges( v ) )
			{
				var otherVertex = edge.Target;
				var otherData = _datas[otherVertex];

			    if (_visitedVertices.Contains(otherVertex)) continue;
			    FirstWalk( otherVertex );
			    data.D = Math.Max( data.D, otherData.R );
			    otherData.A = (float)Math.Atan( ( (float)otherData.R ) / ( data.D + otherData.R ) );
			    s += otherData.A;
			}


			AdjustChildren( v, data, s );
			SetRadius( v, data );
		}

		private void SecondWalk( TVertex v, TVertex r, double x, double y, float l, float t )
		{
			var pos = new Point( x, y );
			VertexPositions[v] = pos;
			_visitedVertices.Add( v );
			var data = _datas[v];

			float dd = l * data.D;
			float p = (float)( t + Math.PI );
			int degree = VisitedGraph.OutDegree( v );
			float fs = ( degree == 0 ? 0 : data.F / degree );
			float pr = 0;

			foreach ( var edge in VisitedGraph.OutEdges( v ) )
			{
				var otherVertex = edge.Target;
				if ( _visitedVertices.Contains( otherVertex ) )
					continue;

				var otherData = _datas[otherVertex];
				float aa = data.C * otherData.A;
				float rr = (float)( data.D * Math.Tan( aa ) / ( 1 - Math.Tan( aa ) ) );
				p += pr + aa + fs;

				float xx = (float)( ( l * rr + dd ) * Math.Cos( p ) );
				float yy = (float)( ( l * rr + dd ) * Math.Sign( p ) );
				pr = aa; ;
				SecondWalk( otherVertex, v, x + xx, y + yy, l * data.C, p );
			}
		}

		private void SetRadius( TVertex v, BalloonData data )
		{
			data.R = (int)Math.Max( data.D / 2, Parameters.minRadius );
		}

		private void AdjustChildren( TVertex v, BalloonData data, float s )
		{
			if ( s > Math.PI )
			{
				data.C = (float)Math.PI / s;
				data.F = 0;
			}
			else
			{
				data.C = 1;
				data.F = (float)Math.PI - s;
			}
		}

		private void InitializeData()
		{
			foreach ( var v in VisitedGraph.Vertices )
				_datas[v] = new BalloonData();

			_visitedVertices.Clear();
		}
	}
}
