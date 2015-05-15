using System;
using GraphX.Measure;

namespace GraphX.PCL.Logic.Algorithms.EdgeRouting
{
    /// <summary>
    /// Used for vector calculations
    /// </summary>
    internal static class VectorTools
    {
        public static Point Minus(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point Plus(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point Multiply(Point p, double f)
        {
            return new Point(p.X * f, p.Y * f);
        }

        public static Point MidPoint(Point p1, Point p2)
        {
            //CHANGED
            return Multiply(Plus(p1, p2), 0.5f);
        }

        public static float Length(Point p)
        {
            return (float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public static Point Normalize(Point p)
        {
            float l = Length(p);
            if (l == 0)
                return p;
            else
                return Multiply(p, 1f / l);
        }

        public static float Angle(Point p1, Point p2, Point q1, Point q2)
        {
            return (float)(Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) - Math.Atan2(q2.Y - q1.Y, q2.X - q1.X));
        }

        public static float Distance(Point p, Point q)
        {
            //CHANGED
            return Length(Minus(q, p));
        }
    }
}
