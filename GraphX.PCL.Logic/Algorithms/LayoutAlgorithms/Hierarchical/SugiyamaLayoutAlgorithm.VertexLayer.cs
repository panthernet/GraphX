using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public partial class SugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
		private class VertexLayer : List<SugiVertex>
		{
			#region Properties, fields
			/// <summary>
			/// Index of the layer.
			/// </summary>
			public int LayerIndex { get; set; }

			public readonly SoftMutableHierarchicalGraph<SugiVertex, SugiEdge> Graph;

			/// <summary>
			/// Height of the layer. (Equals with the height of the heightest vertex.)
			/// </summary>
			public double Height { get { return ComputeHeight(); } }

			/// <summary>
			/// List of the hierarchical edges comes into this layer.
			/// </summary>
			public IEnumerable<SugiEdge> UpEdges
			{
				get {
				    return this.SelectMany(v => Graph.InHierarchicalEdges( v ));
				}
			}

			/// <summary>
			/// List of the hierarhical edges goes out from this layer.
			/// </summary>
			public IEnumerable<SugiEdge> DownEdges
			{
				get {
				    return this.SelectMany(v => Graph.OutHierarchicalEdges( v ));
				}
			}
			#endregion

			#region Constructors

			public VertexLayer(
				SoftMutableHierarchicalGraph<SugiVertex, SugiEdge> graph,
				int layerIndex,
				IEnumerable<SugiVertex> vertices )
			{
				Graph = graph;
				LayerIndex = layerIndex;
				AddRange( vertices );
			}

			#endregion

			#region Crosscounting
			public int CalculateCrossCount( CrossCount crossCountDirection )
			{
				return CalculateCrossCount( crossCountDirection, false, false );
			}

			public int CalculateCrossCount( CrossCount crossCountDirection, bool sourcesByMeasure, bool targetsByMeasure )
			{
				int crossCount = 0;

				bool calculateUpCrossings = ( crossCountDirection & CrossCount.Up ) == CrossCount.Up;
				bool calculateDownCrossings = ( crossCountDirection & CrossCount.Down ) == CrossCount.Down;

				if ( calculateUpCrossings )
					crossCount += CalculateCrossings( UpEdges, sourcesByMeasure, targetsByMeasure );

				if ( calculateDownCrossings )
					crossCount += CalculateCrossings( DownEdges, sourcesByMeasure, targetsByMeasure );

				return crossCount;
			}

			private static int CalculateCrossings( IEnumerable<SugiEdge> edges, bool sourcesByMeasure, bool targetsByMeasure )
			{
				var edgeArray = edges.ToArray();
				int count = edgeArray.Length;
				int crossings = 0;
			    for ( int i = 0; i < count; i++ )
			    {
			        var edge1 = edgeArray[i];
			        for ( int j = i + 1; j < count; j++ )
					{
						var edge2 = edgeArray[j];
						Debug.Assert(
							( edge1.Source.LayerIndex == edge2.Source.LayerIndex &&
							  edge1.Target.LayerIndex == edge2.Target.LayerIndex ),
							"Bad edge at crossing computing: " + edge1 + "\n" + edge2 );

						//get the position of the sources
						double source2Pos;
						double source1Pos;
						if ( sourcesByMeasure )
						{
							source1Pos = edge1.Source.Measure;
							source2Pos = edge2.Source.Measure;
						}
						else
						{
							source1Pos = edge1.Source.Position;
							source2Pos = edge2.Source.Position;
						}

						//get the position of the targets
						double target1Pos;
						double target2Pos;
						if ( targetsByMeasure )
						{
							target1Pos = edge1.Target.Measure;
							target2Pos = edge2.Target.Measure;
						}
						else
						{
							target1Pos = edge1.Target.Position;
							target2Pos = edge2.Target.Position;
						}

						if ( ( source1Pos - source2Pos ) * ( target1Pos - target2Pos ) < 0 )
							crossings++;
					}
			    }
			    return crossings;
			}
			#endregion

			#region Insert & Remove

			public new void Add( SugiVertex vertex )
			{
				base.Add( vertex );
				vertex.LayerIndex = LayerIndex;
				ReassignPositions();
			}

			public new void AddRange( IEnumerable<SugiVertex> vertices )
			{
				base.AddRange( vertices );
				foreach ( var v in vertices )
					v.LayerIndex = LayerIndex;
				ReassignPositions();
			}

			public new void Remove( SugiVertex vertex )
			{
				base.Remove( vertex );
				vertex.LayerIndex = SugiVertex.UNDEFINED_LAYER_INDEX;
			}

			#endregion

			#region Measuring

			/// <summary>
			/// Computes the measures for every vertex in the layer by the given barycenters.
			/// </summary>
			/// <param name="baryCenters">The barycenters.</param>
			/// <param name="byRealPosition">If true, the barycenters will be computed based on the RealPosition.X value of the vertices. Otherwise the barycenter will be computed based on the value of the Position field (which is basically the index of the vertex inside the layer).</param>
			public void Measure( BaryCenter baryCenters, bool byRealPosition )
			{
				bool computeUpBaryCenter = ( baryCenters & BaryCenter.Up ) == BaryCenter.Up;
				bool computeDownBaryCenter = ( baryCenters & BaryCenter.Down ) == BaryCenter.Down;
				bool computeSubBaryCenter = ( baryCenters & BaryCenter.Sub ) == BaryCenter.Sub;

				int divCount = 0;
				if ( computeUpBaryCenter )
					divCount++;
				if ( computeDownBaryCenter )
					divCount++;
				if ( computeSubBaryCenter )
					divCount++;

				//compute the measures for every vertex in the layer
				foreach ( var vertex in this )
					Measure( vertex, computeUpBaryCenter, computeDownBaryCenter, computeSubBaryCenter, divCount, byRealPosition );
			}

			/// <summary>
			/// Computes the measure for the given <paramref name="vertex"/>.
			/// </summary>
			/// <param name="vertex"></param>
			/// <param name="computeUpBaryCenter"></param>
			/// <param name="computeDownBaryCenter"></param>
			/// <param name="computeSubBaryCenter"></param>
			/// <param name="divCount"></param>
			/// <param name="byRealPosition"></param>
			private void Measure( SugiVertex vertex, bool computeUpBaryCenter, bool computeDownBaryCenter, bool computeSubBaryCenter, int divCount, bool byRealPosition )
			{
				vertex.Measure = 0;

				if ( computeUpBaryCenter )
					vertex.Measure += ComputeBaryCenter( vertex, Graph.InHierarchicalEdges( vertex ), byRealPosition );
				if ( computeDownBaryCenter )
					vertex.Measure += ComputeBaryCenter( vertex, Graph.OutHierarchicalEdges( vertex ), byRealPosition );
				if ( computeSubBaryCenter )
					vertex.Measure += ComputeBaryCenter( vertex, Graph.GeneralEdgesFor( vertex ), byRealPosition );

				vertex.Measure /= divCount;
			}

			/// <summary>
			/// Computes the barycenter of the given <paramref name="vertex"/>
			/// based on positions of the vertices on other side of the given <paramref name="edges"/>.
			/// </summary>
			/// <param name="vertex">The vertex which barycenter will be computed.</param>
			/// <param name="edges">The edges used for the computation.</param>
			/// <param name="byRealPosition"></param>
			/// <returns>The computed barycenter.</returns>
			private static double ComputeBaryCenter( SugiVertex vertex, IEnumerable<SugiEdge> edges, bool byRealPosition )
			{
				double baryCenter = 0;
				int number = 0;

				foreach ( var edge in edges )
				{
					if ( byRealPosition )
						baryCenter += edge.OtherVertex( vertex ).RealPosition.X;
					else
						baryCenter += edge.OtherVertex( vertex ).Position;
					number++;
				}

				if ( number != 0 )
					return baryCenter / number;

				return ( byRealPosition ? vertex.RealPosition.X : vertex.Position );
			}

			/// <summary>
			/// Computes the height of the vertexlayer (which is the maximum height of the vertices 
			/// in this layer).
			/// </summary>
			/// <returns>Returns with the computed height of the layer.</returns>
			private double ComputeHeight()
			{
				return this.Max( v => v.Size.Height );
			}
			#endregion

			#region Sort
			/// <summary>
			/// 
			/// </summary>
			/// <param name="baryCenters"></param>
			/// <param name="byRealPosition"></param>
			/// <returns>Returns with true if the vertices in this 
			/// layer ordered by the given <paramref name="baryCenters"/>.</returns>
			public bool IsOrderedByBaryCenters( BaryCenter baryCenters, bool byRealPosition )
			{
				if ( Count == 0 ) return true;

				//fill the measure by the given barycenters
				Measure( baryCenters, byRealPosition );

				//check that the ordering is valid
				for ( int i = 1; i < Count; i++ )
				{
					if ( this[i].Measure < this[i - 1].Measure )
						return false; //invalid ordering
				}

				//the ordering is valid
				return true;
			}

			/// <summary>
			/// Sort the vertices in the layer by it's measures.
			/// </summary>
			public void SortByMeasure()
			{
				//sort the vertices by the measure
				Sort( MeasureComparer.Instance );

				//reassing the positions of the vertices
				ReassignPositions();
			}

			protected void SavePositionsToTemp()
			{
				foreach ( var v in this )
					v.Temp = v.Position;
			}

			protected void LoadPositionsFromTemp()
			{
				foreach ( var v in this )
					v.Position = (int)v.Temp;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="vertices"></param>
			/// <returns>Returns true if the vertices have permutated, 
			/// otherwise (no more permutation) returns with false.</returns>
			protected bool Permutate( IList<SugiVertex> vertices )
			{
				//do the initial ordering
				int n = vertices.Count;
				int i, j;

				//find place to start
				for ( i = n - 1;
					  i > 0 && vertices[i - 1].PermutationIndex >= vertices[i].PermutationIndex;
					  i-- ) { }

				//all in reverse order
				if ( i < 1 )
					return false; //no more permutation

				//do next permutation
				for ( j = n;
					  j > 1 && vertices[j - 1].PermutationIndex <= vertices[i - 1].PermutationIndex;
					  j-- ) { }

				//swap values i-1, j-1
				int c = vertices[i - 1].PermutationIndex;
				vertices[i - 1].PermutationIndex = vertices[j - 1].PermutationIndex;
				vertices[j - 1].PermutationIndex = c;

				//need more swaps
				for ( i++, j = n; i < j; i++, j-- )
				{
					c = vertices[i - 1].PermutationIndex;
					vertices[i - 1].PermutationIndex = vertices[j - 1].PermutationIndex;
					vertices[j - 1].PermutationIndex = c;
				}

				return true; //new permutation generated
			}

			/// <summary>
			/// Changes the order of the vertices with the same measure.
			/// It does that in the brute-force way (every permutation will be analyzed).
			/// Vertices should be sorted by it's measures.
			/// </summary>
			public void FindBestPermutation( CrossCount crossCounting )
			{
				int bestKnownCrossCount = CalculateCrossCount( crossCounting );

				//get the vertices with the same index
				var verticesWithSameMeasure = new List<SugiVertex>();
				int startIndex, endIndex;
				for ( startIndex = 0; startIndex < Count; startIndex = endIndex + 1 )
				{
					for ( endIndex = startIndex + 1;
						  endIndex < Count && this[startIndex].Measure == this[endIndex].Measure;
						  endIndex++ ) { }
					endIndex -= 1;

					if ( endIndex > startIndex )
					{
						for ( int i = startIndex; i <= endIndex; i++ )
							verticesWithSameMeasure.Add( this[i] );
					}
				}

				//save the original positions
				SavePositionsToTemp();

				//null PermutationIndex
				foreach ( var v in this )
					v.PermutationIndex = 0;

				//create initial permutation
				for ( int i = 0; i < verticesWithSameMeasure.Count; i++ )
					verticesWithSameMeasure[i].PermutationIndex = 0;

				while ( Permutate( verticesWithSameMeasure ) )
				{
					//sort the vertices with the same measure by barycenter
					Sort( MeasureAndPermutationIndexComparer.Instance );
					ReassignPositions();

					int newCrossCount = CalculateCrossCount( crossCounting );
					if ( newCrossCount < bestKnownCrossCount )
					{
						SavePositionsToTemp();
						bestKnownCrossCount = newCrossCount;
					}
				}

				//the best solution is in the temp
				LoadPositionsFromTemp();

				Sort( PositionComparer.Instance );
				ReassignPositions();
			}

			/// <summary>
			/// Reassigns the position of vertices to it's indexes in the vertexlayer.
			/// </summary>
			public void ReassignPositions()
			{
				int index = 0;
				foreach ( SugiVertex v in this )
					v.Position = index++;
			}
			#endregion

			class MeasureComparer : IComparer<SugiVertex>
			{
				private static MeasureComparer _instance;
				public static MeasureComparer Instance
				{
					get { return _instance ?? (_instance = new MeasureComparer()); }
				}

				private MeasureComparer() { }

				public int Compare( SugiVertex x, SugiVertex y )
				{
					return Math.Sign( (sbyte)( x.Measure - y.Measure ) );
				}
			}

			class PositionComparer : IComparer<SugiVertex>
			{
				private static PositionComparer _instance;
				public static PositionComparer Instance
				{
					get { return _instance ?? (_instance = new PositionComparer()); }
				}

				private PositionComparer() { }

				public int Compare( SugiVertex x, SugiVertex y )
				{
					return Math.Sign( (sbyte)( x.Position - y.Position ) );
				}
			}

			class MeasureAndPermutationIndexComparer : IComparer<SugiVertex>
			{
				private static MeasureAndPermutationIndexComparer _instance;
				public static MeasureAndPermutationIndexComparer Instance
				{
					get { return _instance ?? (_instance = new MeasureAndPermutationIndexComparer()); }
				}

				private MeasureAndPermutationIndexComparer() { }

				public int Compare( SugiVertex x, SugiVertex y )
				{
					int sign = Math.Sign( (sbyte)( x.Measure - y.Measure ) );
					if ( sign == 0 )
						return Math.Sign( (sbyte)( x.PermutationIndex - y.PermutationIndex ) );

					return sign;
				}
			}

			#region Priorities
			public void CalculateSubPriorities()
			{
				var orderedVertices = ( from v in this
										orderby v.Priority ascending, v.Measure ascending, v.Position ascending
										select v ).ToArray();

				//calculate subpriorities
				int startIndex = 0;
				while ( startIndex < orderedVertices.Length )
				{
					int endIndex = startIndex + 1;

					//get the vertices with the same priorities and measure
					while ( endIndex < orderedVertices.Length
							&& orderedVertices[startIndex].Priority == orderedVertices[endIndex].Priority
							&& orderedVertices[startIndex].Measure == orderedVertices[endIndex].Measure )
						endIndex++;
					endIndex--;

					//set the subpriorities
					int count = endIndex - startIndex + 1;
					var border = (int)Math.Ceiling( count / (float)2.0 );
					int subPriority = count - border;
					for ( int i = 0; i < count; i++ )
					{
						orderedVertices[startIndex + i].SubPriority = count - Math.Abs( subPriority );
						subPriority--;
					}

					//go to the next group of vertices with the same priorities
					startIndex = endIndex + 1;
				}
			}
			#endregion
		}
	}
}