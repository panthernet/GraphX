using System;
using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;
using System.Linq;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.FDP
{
	public partial class LinLogLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, LinLogLayoutParameters>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
		#region Constructors
		public LinLogLayoutAlgorithm( TGraph visitedGraph )
			: base( visitedGraph ) { }

		public LinLogLayoutAlgorithm( TGraph visitedGraph, IDictionary<TVertex, Point> positions,
		                              LinLogLayoutParameters parameters )
			: base( visitedGraph, positions, parameters ) 
        { 
        }
		#endregion

		#region Member variables - privates
		class LinLogVertex
		{
			public int Index;
			public TVertex OriginalVertex;
			public LinLogEdge[] Attractions;
			public double RepulsionWeight;
			public Point Position;
		}

		class LinLogEdge
		{
			public LinLogVertex Target;
			public double AttractionWeight;
		}

		private LinLogVertex[] vertices;
		private Point baryCenter;
		private double repulsionMultiplier;

		#endregion


		protected override void InternalCompute()
		{
			if ( VisitedGraph.VertexCount <= 1 ) return;

			InitializeWithRandomPositions( 1, 1, -0.5, -0.5 );

			InitAlgorithm();
			QuadTree quadTree;

			double finalRepuExponent = Parameters.repulsiveExponent;
			double finalAttrExponent = Parameters.attractionExponent;

			for ( int step = 1; step <= Parameters.iterationCount; step++ )
			{
				ComputeBaryCenter();
				quadTree = BuildQuadTree();

				#region hûlési függvény meghatározása
				if ( Parameters.iterationCount >= 50 && finalRepuExponent < 1.0 )
				{
					Parameters.attractionExponent = finalAttrExponent;
					Parameters.repulsiveExponent = finalRepuExponent;
					if ( step <= 0.6 * Parameters.iterationCount )
					{
						// use energy model with few local minima 
						Parameters.attractionExponent += 1.1 * ( 1.0 - finalRepuExponent );
						Parameters.repulsiveExponent += 0.9 * ( 1.0 - finalRepuExponent );
					}
					else if ( step <= 0.9 * Parameters.iterationCount )
					{
						// gradually move to final energy model
						Parameters.attractionExponent +=
							1.1 * ( 1.0 - finalRepuExponent ) * ( 0.9 - step / (double)Parameters.iterationCount ) / 0.3;
						Parameters.repulsiveExponent +=
							0.9 * ( 1.0 - finalRepuExponent ) * ( 0.9 - step / (double)Parameters.iterationCount ) / 0.3;
					}
				}
				#endregion

				#region Move each node
				for ( int i = 0; i < vertices.Length; i++ )
				{
					var v = vertices[i];
					double oldEnergy = GetEnergy( i, quadTree );

					// compute direction of the move of the node
					Vector bestDir;
					GetDirection( i, quadTree, out bestDir );

					// line search: compute length of the move
					Point oldPos = v.Position;

					double bestEnergy = oldEnergy;
					int bestMultiple = 0;
					bestDir /= 32;
					//kisebb mozgatások esetén a legjobb eset meghatározása
					for ( int multiple = 32;
					      multiple >= 1 && ( bestMultiple == 0 || bestMultiple / 2 == multiple );
					      multiple /= 2 )
					{
						v.Position = oldPos + bestDir * multiple;
						double curEnergy = GetEnergy( i, quadTree );
						if ( curEnergy < bestEnergy )
						{
							bestEnergy = curEnergy;
							bestMultiple = multiple;
						}
					}

					//nagyobb mozgatás esetén van-e jobb megoldás?
					for ( int multiple = 64;
					      multiple <= 128 && bestMultiple == multiple / 2;
					      multiple *= 2 )
					{
						v.Position = oldPos + bestDir * multiple;
						double curEnergy = GetEnergy( i, quadTree );
						if ( curEnergy < bestEnergy )
						{
							bestEnergy = curEnergy;
							bestMultiple = multiple;
						}
					}

					//legjobb megoldással mozgatás
					v.Position = oldPos + bestDir * bestMultiple;
					if ( bestMultiple > 0 )
					{
						quadTree.MoveNode( oldPos, v.Position, v.RepulsionWeight );
					}
				}
				#endregion

				/*if ( ReportOnIterationEndNeeded )
					Report( step );*/
			}
			CopyPositions();
			NormalizePositions();
		}

		protected void CopyPositions()
		{
			// Copy positions
			foreach ( var v in vertices )
				VertexPositions[v.OriginalVertex] = v.Position;
		}

		protected void Report( int step )
		{
			CopyPositions();
			//OnIterationEnded( step, step / (double)Parameters.iterationCount * 100, "Iteration " + step + " finished.", true );
		}

		private void GetDirection( int index, QuadTree quadTree, out Vector dir )
		{
			dir = new Vector( 0, 0 );

			double dir2 = AddRepulsionDirection( index, quadTree, ref dir );
			dir2 += AddAttractionDirection( index, ref dir );
			dir2 += AddGravitationDirection( index, ref dir );

			if ( dir2 != 0.0 )
			{
				dir /= dir2;

				double length = dir.Length;
				if ( length > quadTree.Width / 8 )
				{
					length /= quadTree.Width / 8;
					dir /= length;
				}
			}
			else { dir = new Vector( 0, 0 ); }
		}

		private double AddGravitationDirection( int index, ref Vector dir )
		{
			var v = vertices[index];
			Vector gravitationVector = ( baryCenter - v.Position );
			double dist = gravitationVector.Length;
			double tmp = Parameters.gravitationMultiplier * repulsionMultiplier * Math.Max( v.RepulsionWeight, 1 ) * Math.Pow( dist, Parameters.attractionExponent - 2 );
			dir += gravitationVector * tmp;

			return tmp * Math.Abs( Parameters.attractionExponent - 1 );
		}

		private double AddAttractionDirection( int index, ref Vector dir )
		{
			double dir2 = 0.0;
			var v = vertices[index];
			foreach ( var e in v.Attractions )
			{
				//onhurkok elhagyasa
				if ( e.Target == v )
					continue;

				Vector attractionVector = ( e.Target.Position - v.Position );
				double dist = attractionVector.Length;
				if ( dist <= 0 )
					continue;

				double tmp = e.AttractionWeight * Math.Pow( dist, Parameters.attractionExponent - 2 );
				dir2 += tmp * Math.Abs( Parameters.attractionExponent - 1 );

				dir += ( e.Target.Position - v.Position ) * tmp;
			}
			return dir2;
		}

		/// <summary>
		/// Kiszámítja az <code>index</code> sorszámú pontra ható erõt a 
		/// quadTree segítségével.
		/// </summary>
		/// <param name="index">A node sorszáma, melyre a repulzív erõt számítani akarjuk.</param>
		/// <param name="quadTree"></param>
		/// <param name="dir">A repulzív erõt hozzáadja ehhez a Vectorhoz.</param>
		/// <returns>Becsült második deriváltja a repulzív energiának.</returns>
		private double AddRepulsionDirection( int index, QuadTree quadTree, ref Vector dir )
		{
			var v = vertices[index];

			if ( quadTree == null || quadTree.Index == index || v.RepulsionWeight <= 0 )
				return 0.0;

			Vector repulsionVector = ( quadTree.Position - v.Position );
			double dist = repulsionVector.Length;
			if ( quadTree.Index < 0 && dist < 2.0 * quadTree.Width )
			{
				double dir2 = 0.0;
				for ( int i = 0; i < quadTree.Children.Length; i++ )
					dir2 += AddRepulsionDirection( index, quadTree.Children[i], ref dir );
				return dir2;
			}

			if ( dist != 0.0 )
			{
				double tmp = repulsionMultiplier * v.RepulsionWeight * quadTree.Weight
				             * Math.Pow( dist, Parameters.repulsiveExponent - 2 );
				dir -= repulsionVector * tmp;
				return tmp * Math.Abs( Parameters.repulsiveExponent - 1 );
			}

			return 0.0;
		}

		/*
				private double GetEnergySum( QuadTree q )
				{
					double sum = 0;
					for ( int i = 0; i < vertices.Length; i++ )
						sum += GetEnergy( i, q );
					return sum;
				}
		*/

		private double GetEnergy( int index, QuadTree q )
		{
			return GetRepulsionEnergy( index, q )
			       + GetAttractionEnergy( index ) + GetGravitationEnergy( index );
		}

		private double GetGravitationEnergy( int index )
		{
			var v = vertices[index];

			double dist = ( v.Position - baryCenter ).Length;
			return Parameters.gravitationMultiplier * repulsionMultiplier * Math.Max( v.RepulsionWeight, 1 )
			       * Math.Pow( dist, Parameters.attractionExponent ) / Parameters.attractionExponent;
		}

		private double GetAttractionEnergy( int index )
		{
			double energy = 0.0;
			var v = vertices[index];
			foreach ( var e in v.Attractions )
			{
				if ( e.Target == v )
					continue;

				double dist = ( e.Target.Position - v.Position ).Length;
				energy += e.AttractionWeight * Math.Pow( dist, Parameters.attractionExponent ) / Parameters.attractionExponent;
			}
			return energy;
		}

		private double GetRepulsionEnergy( int index, QuadTree tree )
		{
			if ( tree == null || tree.Index == index || index >= vertices.Length )
				return 0.0;

			var v = vertices[index];

			double dist = ( v.Position - tree.Position ).Length;
			if ( tree.Index < 0 && dist < ( 2 * tree.Width ) )
			{
				double energy = 0.0;
				for ( int i = 0; i < tree.Children.Length; i++ )
					energy += GetRepulsionEnergy( index, tree.Children[i] );

				return energy;
			}

			if ( Parameters.repulsiveExponent == 0.0 )
				return -repulsionMultiplier * v.RepulsionWeight * tree.Weight * Math.Log( dist );

			return -repulsionMultiplier * v.RepulsionWeight * tree.Weight
			       * Math.Pow( dist, Parameters.repulsiveExponent ) / Parameters.repulsiveExponent;
		}

		private void InitAlgorithm()
		{
			vertices = new LinLogVertex[VisitedGraph.VertexCount];

			var vertexMap = new Dictionary<TVertex, LinLogVertex>();

			//vertexek indexelése
			int i = 0;
			foreach ( TVertex v in VisitedGraph.Vertices )
			{
				vertices[i] = new LinLogVertex
				              	{
				              		Index = i,
				              		OriginalVertex = v,
				              		Attractions = new LinLogEdge[VisitedGraph.Degree( v )],
				              		RepulsionWeight = 0,
				              		Position = VertexPositions[v]
				              	};
				vertexMap[v] = vertices[i];
				i++;
			}

			//minden vertex-hez felépíti az attractionWeights, attractionIndexes,
			//és a repulsionWeights struktúrát, valamint átmásolja a pozícióját a VertexPositions-ból
			foreach ( var v in vertices )
			{
				int attrIndex = 0;
				foreach ( var e in VisitedGraph.InEdges( v.OriginalVertex ) )
				{
					double weight = e is WeightedEdge<TVertex> ? ( ( e as WeightedEdge<TVertex> ).Weight ) : 1;
					v.Attractions[attrIndex] = new LinLogEdge
					                           	{
					                           		Target = vertexMap[e.Source],
					                           		AttractionWeight = weight
					                           	};
					//TODO look at this line below
					//v.RepulsionWeight += weight;
					v.RepulsionWeight += 1;
					attrIndex++;
				}

				foreach ( var e in VisitedGraph.OutEdges( v.OriginalVertex ) )
				{
					double weight = e is WeightedEdge<TVertex> ? ( ( e as WeightedEdge<TVertex> ).Weight ) : 1;
					v.Attractions[attrIndex] = new LinLogEdge
					                           	{
					                           		Target = vertexMap[e.Target],
					                           		AttractionWeight = weight
					                           	};
					//v.RepulsionWeight += weight;
					v.RepulsionWeight += 1;
					attrIndex++;
				}
				v.RepulsionWeight = Math.Max( v.RepulsionWeight, Parameters.gravitationMultiplier );
			}

			repulsionMultiplier = ComputeRepulsionMultiplier();
		}

		private void ComputeBaryCenter()
		{
			baryCenter = new Point( 0, 0 );
			double repWeightSum = 0.0;
			foreach ( var v in vertices )
			{
				repWeightSum += v.RepulsionWeight;
				baryCenter.X += v.Position.X * v.RepulsionWeight;
				baryCenter.Y += v.Position.Y * v.RepulsionWeight;
			}
			if ( repWeightSum > 0.0 )
			{
				baryCenter.X /= repWeightSum;
				baryCenter.Y /= repWeightSum;
			}
		}

		private double ComputeRepulsionMultiplier()
		{
			double attractionSum = vertices.Sum( v => v.Attractions.Sum( e => e.AttractionWeight ) );
			double repulsionSum = vertices.Sum( v => v.RepulsionWeight );

			if ( repulsionSum > 0 && attractionSum > 0 )
				return attractionSum / Math.Pow( repulsionSum, 2 ) * Math.Pow( repulsionSum, 0.5 * ( Parameters.attractionExponent - Parameters.repulsiveExponent ) );

			return 1;
		}

		/// <summary>
		/// Felépít egy QuadTree-t (olyan mint az OctTree, csak 2D-ben).
		/// </summary>
		private QuadTree BuildQuadTree()
		{
			//a minimális és maximális pozíció számítása
			var minPos = new Point( double.MaxValue, double.MaxValue );
			var maxPos = new Point( -double.MaxValue, -double.MaxValue );

			foreach ( var v in vertices )
			{
				if ( v.RepulsionWeight <= 0 )
					continue;

				minPos.X = Math.Min( minPos.X, v.Position.X );
				minPos.Y = Math.Min( minPos.Y, v.Position.Y );
				maxPos.X = Math.Max( maxPos.X, v.Position.X );
				maxPos.Y = Math.Max( maxPos.Y, v.Position.Y );
			}

			//a nemnulla repulsionWeight-el rendelkezõ node-ok hozzáadása a QuadTree-hez.
			QuadTree result = null;
			foreach ( var v in vertices )
			{
				if ( v.RepulsionWeight <= 0 )
					continue;

				if ( result == null )
					result = new QuadTree( v.Index, v.Position, v.RepulsionWeight, minPos, maxPos );
				else
					result.AddNode( v.Index, v.Position, v.RepulsionWeight, 0 );
			}
			return result;
		}
	}
}