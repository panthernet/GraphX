using System;
using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;

namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
    public class OneWayFSAAlgorithm<TObject> : FSAAlgorithm<TObject, OneWayFSAParameters>
        where TObject : class
    {
        public OneWayFSAAlgorithm( IDictionary<TObject, Rect> rectangles, OneWayFSAParameters parameters )
            : base( rectangles, parameters )
        {
        }

        protected override void RemoveOverlap(CancellationToken cancellationToken)
        {
            switch ( Parameters.Way )
            {
                case OneWayFSAWayEnum.Horizontal:
                    HorizontalImproved(cancellationToken);
                    break;
                case OneWayFSAWayEnum.Vertical:
                    VerticalImproved(cancellationToken);
                    break;
                default:
                    break;
            }
        }

        protected new double HorizontalImproved(CancellationToken cancellationToken)
        {
            wrappedRectangles.Sort( XComparison );
            int i = 0, n = wrappedRectangles.Count;

            //bal szelso
            var lmin = wrappedRectangles[0];
            double sigma = 0, x0 = lmin.CenterX;
            var gamma = new double[wrappedRectangles.Count];
            var x = new double[wrappedRectangles.Count];
            while ( i < n )
            {
                var u = wrappedRectangles[i];

                //i-vel azonos középponttal rendelkező téglalapok meghatározása
                int k = i;
                for ( int j = i + 1; j < n; j++ )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var v = wrappedRectangles[j];
                    if ( u.CenterX == v.CenterX )
                    {
                        u = v;
                        k = j;
                    }
                    else
                    {
                        break;
                    }
                }
                double g = 0;

                //ne legyenek ugyanabban a pontban
                for ( int z = i + 1; z <= k; z++ )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var v = wrappedRectangles[z];
                    v.Rectangle.X += ( z - i ) * 0.0001;
                }

                //i-k intervallumban lévő téglalapokra erőszámítás a tőlük balra lévőkkel
                if ( u.CenterX > x0 )
                {
                    for ( int m = i; m <= k; m++ )
                    {
                        double ggg = 0;
                        for ( int j = 0; j < i; j++ )
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var f = force( wrappedRectangles[j].Rectangle, wrappedRectangles[m].Rectangle );
                            ggg = Math.Max( f.X + gamma[j], ggg );
                        }
                        var v = wrappedRectangles[m];
                        double gg = v.Rectangle.Left + ggg < lmin.Rectangle.Left ? sigma : ggg;
                        g = Math.Max( g, gg );
                    }
                }
                //megjegyezzük az elemek eltolásást x tömbbe
                //bal szélő elemet újra meghatározzuk
                for ( int m = i; m <= k; m++ )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    gamma[m] = g;
                    var r = wrappedRectangles[m];
                    x[m] = r.Rectangle.Left + g;
                    if ( r.Rectangle.Left < lmin.Rectangle.Left )
                    {
                        lmin = r;
                    }
                }

                //az i-k intervallum négyzeteitől jobbra lévőkkel erőszámítás, legnagyobb erő tárolása
                // delta = max(0, max{f.x(m,j)|i<=m<=k<j<n})
                double delta = 0;
                for ( int m = i; m <= k; m++ )
                {
                    for ( int j = k + 1; j < n; j++ )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var f = force( wrappedRectangles[m].Rectangle, wrappedRectangles[j].Rectangle );
                        if ( f.X > delta )
                        {
                            delta = f.X;
                        }
                    }
                }
                sigma += delta;
                i = k + 1;
            }
            double cost = 0;
            for ( i = 0; i < n; i++ )
            {
                var r = wrappedRectangles[i];
                double oldPos = r.Rectangle.Left;
                double newPos = x[i];

                r.Rectangle.X = newPos;

                double diff = oldPos - newPos;
                cost += diff * diff;
            }
            return cost;
        }
    }
}