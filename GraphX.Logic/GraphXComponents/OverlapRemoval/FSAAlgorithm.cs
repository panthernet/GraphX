using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
    public class FSAAlgorithm<TObject> : FSAAlgorithm<TObject, IOverlapRemovalParameters>
        where TObject : class
    {
        public FSAAlgorithm( IDictionary<TObject, Rect> rectangles, IOverlapRemovalParameters parameters )
            : base( rectangles, parameters )
        {
        }
    }

    /// <summary>
    /// Tim Dwyer által JAVA-ban implementált FSA algoritmus portolása .NET alá.
    /// 
    /// http://adaptagrams.svn.sourceforge.net/viewvc/adaptagrams/trunk/RectangleOverlapSolver/placement/FSA.java?view=markup
    /// </summary>
    public class FSAAlgorithm<TObject, TParam> : OverlapRemovalAlgorithmBase<TObject, TParam>
        where TObject : class
        where TParam : IOverlapRemovalParameters
    {
        public FSAAlgorithm( IDictionary<TObject, Rect> rectangles, TParam parameters )
            : base( rectangles, parameters )
        {
        }

        protected override void RemoveOverlap()
        {
            DateTime t0 = DateTime.Now;
            double cost = HorizontalImproved();
            DateTime t1 = DateTime.Now;

            Debug.WriteLine( "PFS horizontal: cost=" + cost + " time=" + ( t1 - t0 ) );

            t1 = DateTime.Now;
            cost = VerticalImproved();
            DateTime t2 = DateTime.Now;
            Debug.WriteLine( "PFS vertical: cost=" + cost + " time=" + ( t2 - t1 ) );
            Debug.WriteLine( "PFS total: time=" + ( t2 - t0 ) );
        }

        /// <summary>
        /// Megadja a két téglalap kötött fellépõ erõt.
        /// </summary>
        /// <param name="vi">Egyik téglalap.</param>
        /// <param name="vj">Másik téglalap.</param>
        /// <returns></returns>
        protected Vector force( Rect vi, Rect vj )
        {
            var f = new Vector( 0, 0 );
            Vector d = vj.GetCenter() - vi.GetCenter();
            double adx = Math.Abs( d.X );
            double ady = Math.Abs( d.Y );
            double gij = d.Y / d.X;
            double Gij = ( vi.Height + vj.Height ) / ( vi.Width + vj.Width );
            if ( Gij >= gij && gij > 0 || -Gij <= gij && gij < 0 || gij == 0 )
            {
                // vi and vj touch with y-direction boundaries
                f.X = d.X / adx * ( ( vi.Width + vj.Width ) / 2.0 - adx );
                f.Y = f.X * gij;
            }
            if ( Gij < gij && gij > 0 || -Gij > gij && gij < 0 )
            {
                // vi and vj touch with x-direction boundaries
                f.Y = d.Y / ady * ( ( vi.Height + vj.Height ) / 2.0 - ady );
                f.X = f.Y / gij;
            }
            return f;
        }

        protected Vector force2( Rect vi, Rect vj )
        {
            var f = new Vector( 0, 0 );
            Vector d = vj.GetCenter() - vi.GetCenter();
            double gij = d.Y / d.X;
            if ( vi.IntersectsWith( vj ) )
            {
                f.X = ( vi.Width + vj.Width ) / 2.0 - d.X;
                f.Y = ( vi.Height + vj.Height ) / 2.0 - d.Y;
                // in the x dimension
                if ( f.X > f.Y && gij != 0 )
                {
                    f.X = f.Y / gij;
                }
                f.X = Math.Max( f.X, 0 );
                f.Y = Math.Max( f.Y, 0 );
            }
            return f;
        }

        protected int XComparison( RectangleWrapper<TObject> r1, RectangleWrapper<TObject> r2 )
        {
            double r1CenterX = r1.CenterX;
            double r2CenterX = r2.CenterX;

            if ( r1CenterX < r2CenterX )
            {
                return -1;
            }
            if ( r1CenterX > r2CenterX )
            {
                return 1;
            }
            return 0;
        }

        protected void Horizontal()
        {
            wrappedRectangles.Sort( XComparison );
            int i = 0, n = wrappedRectangles.Count;
            while ( i < n )
            {
                // x_i = x_{i+1} = ... = x_k
                int k = i;
                RectangleWrapper<TObject> u = wrappedRectangles[i];
                //TODO plus 1 check
                for ( int j = i + 1; j < n; j++ )
                {
                    RectangleWrapper<TObject> v = wrappedRectangles[j];
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
                // delta = max(0, max{f.x(m,j)|i<=m<=k<j<n})
                double delta = 0;
                for ( int m = i; m <= k; m++ )
                {
                    for ( int j = k + 1; j < n; j++ )
                    {
                        Vector f = force( wrappedRectangles[m].Rectangle, wrappedRectangles[j].Rectangle );
                        if ( f.X > delta )
                        {
                            delta = f.X;
                        }
                    }
                }
                for ( int j = k + 1; j < n; j++ )
                {
                    RectangleWrapper<TObject> r = wrappedRectangles[j];
                    r.Rectangle.Offset( delta, 0 );
                }
                i = k + 1;
            }

        }

        protected double HorizontalImproved()
        {
            if (wrappedRectangles.Count == 0) return 0;
            wrappedRectangles.Sort( XComparison );
            int i = 0, n = wrappedRectangles.Count;

            //bal szelso
            RectangleWrapper<TObject> lmin = wrappedRectangles[0];
            double sigma = 0, x0 = lmin.CenterX;
            var gamma = new double[wrappedRectangles.Count];
            var x = new double[wrappedRectangles.Count];
            while ( i < n )
            {
                RectangleWrapper<TObject> u = wrappedRectangles[i];

                //i-vel azonos középponttal rendelkezõ téglalapok meghatározása
                int k = i;
                for ( int j = i + 1; j < n; j++ )
                {
                    RectangleWrapper<TObject> v = wrappedRectangles[j];
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

                //i-k intervallumban lévõ téglalapokra erõszámítás a tõlük balra lévõkkel
                if ( u.CenterX > x0 )
                {
                    for ( int m = i; m <= k; m++ )
                    {
                        double ggg = 0;
                        for ( int j = 0; j < i; j++ )
                        {
                            Vector f = force( wrappedRectangles[j].Rectangle, wrappedRectangles[m].Rectangle );
                            ggg = Math.Max( f.X + gamma[j], ggg );
                        }
                        RectangleWrapper<TObject> v = wrappedRectangles[m];
                        double gg =
                            v.Rectangle.Left + ggg < lmin.Rectangle.Left
                                ? sigma
                                : ggg;
                        g = Math.Max( g, gg );
                    }
                }
                //megjegyezzük az elemek eltolásást x tömbbe
                //bal szélõ elemet újra meghatározzuk
                for ( int m = i; m <= k; m++ )
                {
                    gamma[m] = g;
                    RectangleWrapper<TObject> r = wrappedRectangles[m];
                    x[m] = r.Rectangle.Left + g;
                    if ( r.Rectangle.Left < lmin.Rectangle.Left )
                    {
                        lmin = r;
                    }
                }

                //az i-k intervallum négyzeteitõl jobbra lévõkkel erõszámítás, legnagyobb erõ tárolása
                // delta = max(0, max{f.x(m,j)|i<=m<=k<j<n})
                double delta = 0;
                for ( int m = i; m <= k; m++ )
                {
                    for ( int j = k + 1; j < n; j++ )
                    {
                        Vector f = force( wrappedRectangles[m].Rectangle, wrappedRectangles[j].Rectangle );
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
                RectangleWrapper<TObject> r = wrappedRectangles[i];
                double oldPos = r.Rectangle.Left;
                double newPos = x[i];

                r.Rectangle.X = newPos;

                double diff = oldPos - newPos;
                cost += diff * diff;
            }
            return cost;
        }

        protected int YComparison( RectangleWrapper<TObject> r1, RectangleWrapper<TObject> r2 )
        {
            double r1CenterY = r1.CenterY;
            double r2CenterY = r2.CenterY;

            if ( r1CenterY < r2CenterY )
            {
                return -1;
            }
            if ( r1CenterY > r2CenterY )
            {
                return 1;
            }
            return 0;
        }

        protected void Vertical()
        {
            wrappedRectangles.Sort( YComparison );
            int i = 0, n = wrappedRectangles.Count;
            while ( i < n )
            {
                // y_i = y_{i+1} = ... = y_k
                int k = i;
                RectangleWrapper<TObject> u = wrappedRectangles[i];
                for ( int j = i; j < n; j++ )
                {
                    RectangleWrapper<TObject> v = wrappedRectangles[j];
                    if ( u.CenterY == v.CenterY )
                    {
                        u = v;
                        k = j;
                    }
                    else
                    {
                        break;
                    }
                }
                // delta = max(0, max{f.y(m,j)|i<=m<=k<j<n})
                double delta = 0;
                for ( int m = i; m <= k; m++ )
                {
                    for ( int j = k + 1; j < n; j++ )
                    {
                        Vector f = force2( wrappedRectangles[m].Rectangle, wrappedRectangles[j].Rectangle );
                        if ( f.Y > delta )
                        {
                            delta = f.Y;
                        }
                    }
                }
                for ( int j = k + 1; j < n; j++ )
                {
                    RectangleWrapper<TObject> r = wrappedRectangles[j];
                    r.Rectangle.Offset( 0, delta );
                }
                i = k + 1;
            }

        }

        protected double VerticalImproved()
        {
            if (wrappedRectangles.Count == 0) return 0;
            wrappedRectangles.Sort( YComparison );
            int i = 0, n = wrappedRectangles.Count;
            RectangleWrapper<TObject> lmin = wrappedRectangles[0];
            double sigma = 0, y0 = lmin.CenterY;
            var gamma = new double[wrappedRectangles.Count];
            var y = new double[wrappedRectangles.Count];
            while ( i < n )
            {
                RectangleWrapper<TObject> u = wrappedRectangles[i];
                int k = i;
                for ( int j = i + 1; j < n; j++ )
                {
                    RectangleWrapper<TObject> v = wrappedRectangles[j];
                    if ( u.CenterY == v.CenterY )
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
                if ( u.CenterY > y0 )
                {
                    for ( int m = i; m <= k; m++ )
                    {
                        double ggg = 0;
                        for ( int j = 0; j < i; j++ )
                        {
                            Vector f = force2( wrappedRectangles[j].Rectangle, wrappedRectangles[m].Rectangle );
                            ggg = Math.Max( f.Y + gamma[j], ggg );
                        }
                        RectangleWrapper<TObject> v = wrappedRectangles[m];
                        double gg =
                            v.Rectangle.Top + ggg < lmin.Rectangle.Top
                                ? sigma
                                : ggg;
                        g = Math.Max( g, gg );
                    }
                }
                for ( int m = i; m <= k; m++ )
                {
                    gamma[m] = g;
                    RectangleWrapper<TObject> r = wrappedRectangles[m];
                    y[m] = r.Rectangle.Top + g;
                    if ( r.Rectangle.Top < lmin.Rectangle.Top )
                    {
                        lmin = r;
                    }
                }
                // delta = max(0, max{f.x(m,j)|i<=m<=k<j<n})
                double delta = 0;
                for ( int m = i; m <= k; m++ )
                {
                    for ( int j = k + 1; j < n; j++ )
                    {
                        Vector f = force( wrappedRectangles[m].Rectangle, wrappedRectangles[j].Rectangle );
                        if ( f.Y > delta )
                        {
                            delta = f.Y;
                        }
                    }
                }
                sigma += delta;
                i = k + 1;
            }

            double cost = 0;
            for ( i = 0; i < n; i++ )
            {
                RectangleWrapper<TObject> r = wrappedRectangles[i];
                double oldPos = r.Rectangle.Top;
                double newPos = y[i];

                r.Rectangle.Y = newPos;

                double diff = oldPos - newPos;
                cost += diff * diff;
            }
            return cost;
        }
    }
}