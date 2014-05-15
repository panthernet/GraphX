using System;
using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.FDP
{
	public class ISOMLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, ISOMLayoutParameters>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
		#region Private fields
		private Queue<TVertex> _queue;
		private Dictionary<TVertex, ISOMData> _isomDataDict;
		private readonly Random _rnd = new Random( DateTime.Now.Millisecond );
		private Point _tempPos;
		private double adaptation;
		private int radius;
		#endregion

		#region Constructors

		public ISOMLayoutAlgorithm( TGraph visitedGraph, ISOMLayoutParameters oldParameters )
			: base( visitedGraph )
		{
			Init( oldParameters );
		}

		public ISOMLayoutAlgorithm( TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions,
		                            ISOMLayoutParameters oldParameters )
			: base( visitedGraph, vertexPositions, oldParameters )
		{
			Init( oldParameters );
		}

		protected void Init( ISOMLayoutParameters oldParameters )
		{
			//init _parameters
			base.InitParameters( oldParameters );

			_queue = new Queue<TVertex>();
			_isomDataDict = new Dictionary<TVertex, ISOMData>();
			adaptation = Parameters.InitialAdaption;
		}
		#endregion

		protected override void InternalCompute()
		{
			//initialize vertex positions
			InitializeWithRandomPositions( Parameters.Width, Parameters.Height );

			//initialize ISOM data
			foreach ( var vertex in VisitedGraph.Vertices )
			{
				ISOMData isomData;
				if ( !_isomDataDict.TryGetValue( vertex, out isomData ) )
				{
					isomData = new ISOMData();
					_isomDataDict[vertex] = isomData;
				}
			}

			radius = Parameters.InitialRadius;
			for ( int epoch = 0; epoch < Parameters.MaxEpoch; epoch++ )
			{
				Adjust();

				//Update Parameters
				double factor = Math.Exp( -1 * Parameters.CoolingFactor * ( 1.0 * epoch / Parameters.MaxEpoch ) );
				adaptation = Math.Max( Parameters.MinAdaption, factor * Parameters.InitialAdaption );
				if ( radius > Parameters.MinRadius && epoch % Parameters.RadiusConstantTime == 0 )
				{
					radius--;
				}

				//report
				/*if ( ReportOnIterationEndNeeded )
					OnIterationEnded( epoch, (double)epoch / (double)Parameters.MaxEpoch, "Iteration " + epoch + " finished.", true );
                else if (ReportOnProgressChangedNeeded)
                    OnProgressChanged( (double)epoch / (double)Parameters.MaxEpoch * 100 );*/
			}
		}

		/// <summary>
		/// Rántsunk egyet az összes ponton.
		/// </summary>
		protected void Adjust()
		{
			_tempPos = new Point();

			//get a random point in the container
			_tempPos.X = 0.1 * Parameters.Width + ( _rnd.NextDouble() * 0.8 * Parameters.Width );
			_tempPos.Y = 0.1 * Parameters.Height + ( _rnd.NextDouble() * 0.8 * Parameters.Height );

			//find the closest vertex to this random point
			TVertex closest = GetClosest( _tempPos );

			//adjust the vertices to the selected vertex
			foreach ( TVertex v in VisitedGraph.Vertices )
			{
				ISOMData vid = _isomDataDict[v];
				vid.Distance = 0;
				vid.Visited = false;
			}
			AdjustVertex( closest );
		}

		private void AdjustVertex( TVertex closest )
		{
			_queue.Clear();
			ISOMData vid = _isomDataDict[closest];
			vid.Distance = 0;
			vid.Visited = true;
			_queue.Enqueue( closest );

			while ( _queue.Count > 0 )
			{
				TVertex current = _queue.Dequeue();
				ISOMData currentVid = _isomDataDict[current];
				Point pos = VertexPositions[current];

				Vector force = _tempPos - pos;
				double factor = adaptation / Math.Pow( 2, currentVid.Distance );

				pos += factor * force;
				VertexPositions[current] = pos;

				//ha még a hatókörön belül van
				if ( currentVid.Distance < radius )
				{
					//akkor a szomszedokra is hatassal vagyunk
					foreach ( TVertex neighbour in VisitedGraph.GetNeighbours<TVertex, TEdge>( current ) )
					{
						ISOMData nvid = _isomDataDict[neighbour];
						if ( !nvid.Visited )
						{
							nvid.Visited = true;
							nvid.Distance = currentVid.Distance + 1;
							_queue.Enqueue( neighbour );
						}
					}
				}
			}
		}

		/// <summary>
		/// Finds the the closest vertex to the given position.
		/// </summary>
		/// <param name="tempPos">The position.</param>
		/// <returns>Returns with the reference of the closest vertex.</returns>
		private TVertex GetClosest( Point tempPos )
		{
			TVertex vertex = default( TVertex );
			double distance = double.MaxValue;

			//find the closest vertex
			foreach ( TVertex v in VisitedGraph.Vertices )
			{
				double d = ( tempPos - VertexPositions[v] ).Length;
				if ( d < distance )
				{
					vertex = v;
					distance = d;
				}
			}
			return vertex;
		}

		private class ISOMData
		{
			public Vector Force = new Vector();
			public bool Visited = false;
			public double Distance = 0.0;
		}
	}
}