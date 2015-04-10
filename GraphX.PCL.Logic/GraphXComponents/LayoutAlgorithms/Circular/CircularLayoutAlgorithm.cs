using System;
using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.Circular
{
    public class CircularLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, CircularLayoutParameters>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        readonly IDictionary<TVertex, Size> sizes;

        public CircularLayoutAlgorithm( TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Size> vertexSizes, CircularLayoutParameters parameters )
            : base( visitedGraph, vertexPositions, parameters )
        {
            //Contract.Requires( vertexSizes != null );
            //Contract.Requires( visitedGraph.Vertices.All( v => vertexSizes.ContainsKey( v ) ) );

            sizes = vertexSizes;
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            //calculate the size of the circle
            double perimeter = 0;
            double[] halfSize = new double[VisitedGraph.VertexCount];
            int i = 0;
            foreach ( var v in VisitedGraph.Vertices )
            {
                cancellationToken.ThrowIfCancellationRequested();

                Size s = sizes[v];
                halfSize[i] = Math.Sqrt( s.Width * s.Width + s.Height * s.Height ) * 0.5;
                perimeter += halfSize[i] * 2;
                i++;
            }

            double radius = perimeter / ( 2 * Math.PI );

            //
            //precalculation
            //
            double angle = 0, a;
            i = 0;
            foreach ( var v in VisitedGraph.Vertices )
            {
                cancellationToken.ThrowIfCancellationRequested();

                a = Math.Sin( halfSize[i] * 0.5 / radius ) * 2;
                angle += a;
                //if ( ReportOnIterationEndNeeded )
                    VertexPositions[v] = new Point( Math.Cos( angle ) * radius + radius, Math.Sin( angle ) * radius + radius );
                angle += a;
            }

            //if ( ReportOnIterationEndNeeded )
            //    OnIterationEnded( 0, 50, "Precalculation done.", false );

            //recalculate radius
            radius = angle / ( 2 * Math.PI ) * radius;

            //calculation
            angle = 0;
            i = 0;
            foreach ( var v in VisitedGraph.Vertices )
            {
                cancellationToken.ThrowIfCancellationRequested();

                a = Math.Sin( halfSize[i] * 0.5 / radius ) * 2;
                angle += a;
                VertexPositions[v] = new Point( Math.Cos( angle ) * radius + radius, Math.Sin( angle ) * radius + radius );
                angle += a;
            }
        }
    }
}