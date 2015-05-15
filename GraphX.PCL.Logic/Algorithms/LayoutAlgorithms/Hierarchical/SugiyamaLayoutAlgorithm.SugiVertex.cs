using System.Diagnostics;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public partial class SugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph> 
        where TVertex : class 
        where TEdge : IEdge<TVertex> 
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
	{
		[DebuggerDisplay( "{Original} [{LayerIndex}] Pos={Position} Meas={Measure} RealPos={RealPosition}" )]
		private class SugiVertex : WrappedVertex<TVertex>
		{
			//Constants
			public const int UNDEFINED_LAYER_INDEX = -1;
			public const int UNDEFINED_POSITION = -1;

			//Private fields
			private int _layerIndex = UNDEFINED_LAYER_INDEX;

			//Public fields
			/// <summary>
			/// The position of the vertex inside the layer.
			/// </summary>
			public int Position;

			/// <summary>
			/// The measure of the vertex (up/down-barycenter/median depends on the implementation).
			/// </summary>
			public double Measure;

			/// <summary>
			/// The real position (x and y coordinates) of the vertex.
			/// </summary>
			public Point RealPosition;

			/// <summary>
			/// Used in the algorithms for temporary storage.
			/// </summary>
			public double Temp;

			/// <summary>
			/// Size of the vertex.
			/// </summary>
			public Size Size;

			/// <summary>
			/// The index of the layer where this vertex belongs to.
			/// </summary>
			public int LayerIndex
			{
				get { return _layerIndex; }
				set
				{
					if ( _layerIndex != value )
					{
						//change the index
						_layerIndex = value;

						//add to the new layer
						if ( _layerIndex == UNDEFINED_LAYER_INDEX )
							Position = UNDEFINED_POSITION;
					}
				}
			}

			/// <summary>
			/// Gets that this vertex is a dummy vertex (a point of a replaced long edge) or not.
			/// </summary>
			public bool IsDummyVertex
			{
				get { return Original == null; }
			}

			/// <summary>
			/// The priority of the vertex. Used in the horizontal position assignment phase.
			/// The dummy vertices has maximal priorities (because the dummy edge should be as vertical as possible).
			/// The other vertices priority based on it's edge count.
			/// </summary>
			public int Priority;

			/// <summary>
			/// Represents the subpriority of this vertex between the vertices with the same priority.
			/// </summary>
			public int SubPriority;

			public int PermutationIndex;

			#region Maybe not needed

			public int LeftGeneralEdgeCount;
			public int RightGeneralEdgeCount;

			#endregion

			/// <summary>
			/// Constructor of the vertex.
			/// </summary>
			/// <param name="originalVertex">The object which is wrapped by this ComplexVertex.</param>
			/// <param name="size">The size of the original vertex.</param>
			public SugiVertex( TVertex originalVertex, Size size )
				: base( originalVertex )
			{
				Size = size;
			}

			public override string ToString()
			{
				return ( Original == null ? "Dummy" : Original.ToString() ) + " [" + LayerIndex + "]";
			}
		}
	}
}