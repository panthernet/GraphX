using System;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public partial class LinLogLayoutAlgorithm<TVertex, TEdge, TGraph> 
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
		class QuadTree
		{
			#region Properties
			private readonly QuadTree[] _children = new QuadTree[4];
			public QuadTree[] Children
			{
				get { return _children; }
			}

		    public int Index { get; private set; }

		    private Point _position;

			public Point Position
			{
				get { return _position; }
			}

		    public double Weight { get; private set; }

		    private Point _minPos;
			private Point _maxPos;

			#endregion

			public double Width
			{
				get
				{
					return Math.Max( _maxPos.X - _minPos.X, _maxPos.Y - _minPos.Y );
				}
			}

		    private const int MAX_DEPTH = 20;

			public QuadTree( int index, Point position, double weight, Point minPos, Point maxPos )
			{
				Index = index;
				_position = position;
				Weight = weight;
				_minPos = minPos;
				_maxPos = maxPos;
			}

			public void AddNode( int nodeIndex, Point nodePos, double nodeWeight, int depth )
			{
				if ( depth > MAX_DEPTH )
					return;

				if ( Index >= 0 )
				{
					AddNode2( Index, _position, Weight, depth );
					Index = -1;
				}

				_position.X = ( _position.X * Weight + nodePos.X * nodeWeight ) / ( Weight + nodeWeight );
				_position.Y = ( _position.Y * Weight + nodePos.Y * nodeWeight ) / ( Weight + nodeWeight );
				Weight += nodeWeight;

				AddNode2( nodeIndex, nodePos, nodeWeight, depth );
			}

		    private void AddNode2( int nodeIndex, Point nodePos, double nodeWeight, int depth )
			{
				//Debug.WriteLine( string.Format( "AddNode2 {0} {1} {2} {3}", nodeIndex, nodePos, nodeWeight, depth ) );
				var childIndex = 0;
				var middleX = ( _minPos.X + _maxPos.X ) / 2;
				var middleY = ( _minPos.Y + _maxPos.Y ) / 2;

				if ( nodePos.X > middleX )
					childIndex += 1;

				if ( nodePos.Y > middleY )
					childIndex += 2;

				//Debug.WriteLine( string.Format( "childIndex: {0}", childIndex ) );               


				if ( _children[childIndex] == null )
				{
					var newMin = new Point();
					var newMax = new Point();
					if ( nodePos.X <= middleX )
					{
						newMin.X = _minPos.X;
						newMax.X = middleX;
					}
					else
					{
						newMin.X = middleX;
						newMax.X = _maxPos.X;
					}
					if ( nodePos.Y <= middleY )
					{
						newMin.Y = _minPos.Y;
						newMax.Y = middleY;
					}
					else
					{
						newMin.Y = middleY;
						newMax.Y = _maxPos.Y;
					}
					_children[childIndex] = new QuadTree( nodeIndex, nodePos, nodeWeight, newMin, newMax );
				}
				else
				{
					_children[childIndex].AddNode( nodeIndex, nodePos, nodeWeight, depth + 1 );
				}
			}

			/// <summary>
			/// Az adott rész pozícióját újraszámítja, levonva belőle a mozgatott node részét.
			/// </summary>
			/// <param name="oldPos"></param>
			/// <param name="newPos"></param>
			/// <param name="nodeWeight"></param>
			public void MoveNode( Point oldPos, Point newPos, double nodeWeight )
			{
				_position += ( ( newPos - oldPos ) * ( nodeWeight / Weight ) );

				var childIndex = 0;
				var middleX = ( _minPos.X + _maxPos.X ) / 2;
				var middleY = ( _minPos.Y + _maxPos.Y ) / 2;

				if ( oldPos.X > middleX )
					childIndex += 1;
				if ( oldPos.Y > middleY )
					childIndex += 1 << 1;

				if ( _children[childIndex] != null )
					_children[childIndex].MoveNode( oldPos, newPos, nodeWeight );
			}
		}
	}
}