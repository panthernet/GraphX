using System;
using System.Windows;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.FDP
{
	public partial class LinLogLayoutAlgorithm<TVertex, TEdge, TGraph> 
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : IBidirectionalGraph<TVertex, TEdge>
	{
		class QuadTree
		{
			#region Properties
			private readonly QuadTree[] children = new QuadTree[4];
			public QuadTree[] Children
			{
				get { return children; }
			}

			private int index;
			public int Index
			{
				get { return index; }
			}

			private Point position;

			public Point Position
			{
				get { return position; }
			}

			private double weight;

			public double Weight
			{
				get { return weight; }
			}

			private Point minPos;
			private Point maxPos;

			#endregion

			public double Width
			{
				get
				{
					return Math.Max( maxPos.X - minPos.X, maxPos.Y - minPos.Y );
				}
			}

			protected const int maxDepth = 20;

			public QuadTree( int index, Point position, double weight, Point minPos, Point maxPos )
			{
				this.index = index;
				this.position = position;
				this.weight = weight;
				this.minPos = minPos;
				this.maxPos = maxPos;
			}

			public void AddNode( int nodeIndex, Point nodePos, double nodeWeight, int depth )
			{
				if ( depth > maxDepth )
					return;

				if ( index >= 0 )
				{
					AddNode2( index, position, weight, depth );
					index = -1;
				}

				position.X = ( position.X * weight + nodePos.X * nodeWeight ) / ( weight + nodeWeight );
				position.Y = ( position.Y * weight + nodePos.Y * nodeWeight ) / ( weight + nodeWeight );
				weight += nodeWeight;

				AddNode2( nodeIndex, nodePos, nodeWeight, depth );
			}

			protected void AddNode2( int nodeIndex, Point nodePos, double nodeWeight, int depth )
			{
				//Debug.WriteLine( string.Format( "AddNode2 {0} {1} {2} {3}", nodeIndex, nodePos, nodeWeight, depth ) );
				int childIndex = 0;
				double middleX = ( minPos.X + maxPos.X ) / 2;
				double middleY = ( minPos.Y + maxPos.Y ) / 2;

				if ( nodePos.X > middleX )
					childIndex += 1;

				if ( nodePos.Y > middleY )
					childIndex += 2;

				//Debug.WriteLine( string.Format( "childIndex: {0}", childIndex ) );               


				if ( children[childIndex] == null )
				{
					var newMin = new Point();
					var newMax = new Point();
					if ( nodePos.X <= middleX )
					{
						newMin.X = minPos.X;
						newMax.X = middleX;
					}
					else
					{
						newMin.X = middleX;
						newMax.X = maxPos.X;
					}
					if ( nodePos.Y <= middleY )
					{
						newMin.Y = minPos.Y;
						newMax.Y = middleY;
					}
					else
					{
						newMin.Y = middleY;
						newMax.Y = maxPos.Y;
					}
					children[childIndex] = new QuadTree( nodeIndex, nodePos, nodeWeight, newMin, newMax );
				}
				else
				{
					children[childIndex].AddNode( nodeIndex, nodePos, nodeWeight, depth + 1 );
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
				position += ( ( newPos - oldPos ) * ( nodeWeight / weight ) );

				int childIndex = 0;
				double middleX = ( minPos.X + maxPos.X ) / 2;
				double middleY = ( minPos.Y + maxPos.Y ) / 2;

				if ( oldPos.X > middleX )
					childIndex += 1;
				if ( oldPos.Y > middleY )
					childIndex += 1 << 1;

				if ( children[childIndex] != null )
					children[childIndex].MoveNode( oldPos, newPos, nodeWeight );
			}
		}
	}
}