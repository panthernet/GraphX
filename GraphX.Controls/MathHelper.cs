using System;
#if WPF
using System.Windows;
#elif METRO
using GraphX.Measure;
using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;
#endif

namespace GraphX.Controls
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// Convert angle value to degrees
        /// </summary>
        public static double ToDegrees(this double value)
        {
            return MathHelper.ToDegrees(value);
        }
    }

    public static class MathHelper
    {
        const double D30_DEGREES_IN_RADIANS = Math.PI / 6.0;

        public static double Tangent30Degrees { get; private set; }

        static MathHelper()
        {
            Tangent30Degrees = Math.Tan(D30_DEGREES_IN_RADIANS);
        }

        public static Vector GetDirection(Point from, Point to)
        {
            var dir = new Vector(from.X - to.X, from.Y - to.Y);
            dir.Normalize();
            return dir;
        }


        public static double ToDegrees(double value)
        {
            return value * 180 / Math.PI;
        }
        
        public static double GetAngleBetweenPoints(Point point1, Point point2)
        {
            return Math.Atan2(point1.Y - point2.Y, point2.X - point1.X);
        }

        public static double GetDistanceBetweenPoints(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }


        public static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            var angleInRadians = angleInDegrees * (Math.PI / 180);
            var cosTheta = Math.Cos(angleInRadians);
            var sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        public static bool IsIntersected(Rect r, Point a, Point b)
        {
           // var start = new Point(a.X, a.Y);
            /* line endpoints */
            var codeA = GetIntersectionData(r, a);
            var codeB = GetIntersectionData(r, b);

            if (codeA.IsInside() && codeB.IsInside())
                return true;

            /* while one of the endpoints are outside of rectangle */
            while (!codeA.IsInside() || !codeB.IsInside())
            {
                /* if both points are at one rectangle side then line do not cross the rectangle */
                if (codeA.SameSide(codeB))
                    return false;

                /* select point with zero code */
                sides code;
                Point c; /* one of the points */
                if (!codeA.IsInside())
                {
                    code = codeA;
                    c = a;
                }
                else
                {
                    code = codeB;
                    c = b;
                }

                /* if c is on the left of r then move c on the line x = r->x_min
                   if c is on the right side of r then move c on the line x = r->x_max */
                if (code.Left)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if (code.Right)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }/* if c is below r then move c on the line y = r->y_min
                    if c above the r then move c on the line y = r->y_max */
                else if (code.Bottom)
                {
                    c.X += (a.X - b.X) * (r.Bottom - c.Y) / (a.Y - b.Y);
                    c.Y = r.Bottom;
                }
                else if (code.Top)
                {
                    c.X += (a.X - b.X) * (r.Top - c.Y) / (a.Y - b.Y);
                    c.Y = r.Top;
                }

                /* refresh code */
                if (code == codeA)
                {
                    a = c;
                    codeA = GetIntersectionData(r, a);
                }
                else
                {
                    b = c;
                    codeB = GetIntersectionData(r, b);
                }
            }
            return true;
        }

        public static int GetIntersectionPoint(Rect r, Point a, Point b, out Point pt)
        {
            var start = new Point(a.X, a.Y);

            var codeA = GetIntersectionData(r, a);
            var codeB = GetIntersectionData(r, b);

            while (!codeA.IsInside() || !codeB.IsInside())
            {
                if (codeA.SameSide(codeB))
                {
                    pt = new Point();
                    return -1;
                }

                sides code;
                Point c; 
                if (!codeA.IsInside())
                {
                    code = codeA;
                    c = a;
                }
                else
                {
                    code = codeB;
                    c = b;
                }

                if (code.Left)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if (code.Right)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }
                else if (code.Bottom)
                {
                    c.X += (a.X - b.X) * (r.Bottom - c.Y) / (a.Y - b.Y);
                    c.Y = r.Bottom;
                }
                else if (code.Top)
                {
                    c.X += (a.X - b.X) * (r.Top - c.Y) / (a.Y - b.Y);
                    c.Y = r.Top;
                }

                if (code == codeA)
                {
                    a = c;
                    codeA = GetIntersectionData(r, a);
                }
                else
                {
                    b = c;
                    codeB = GetIntersectionData(r, b);
                }
            }
            pt = GetCloserPoint(start, a, b);
            return 0;
        }

        public sealed class sides
        {
            public bool Left;
            public bool Right;
            public bool Top;
            public bool Bottom;

            public bool IsInside()
            {
                return Left == false && Right == false && Top == false && Bottom == false;
            }

            public bool SameSide(sides o)
            {
                return (Left && o.Left) || (Right && o.Right) || (Top && o.Top)
                    || (Bottom && o.Bottom);
            }
        }

        public static sides GetIntersectionData(Rect r, Point p)
        {
            return new sides() { Left = p.X < r.Left, Right = p.X > r.Right, Bottom = p.Y > r.Bottom, Top = p.Y < r.Top };
        }

        public static double GetDistance(Point a, Point b)
        {
            return ((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public static Point GetCloserPoint(Point start, Point a, Point b)
        {
            var r1 = GetDistance(start, a);
            var r2 = GetDistance(start, b);
            return r1 < r2 ? a : b;
        }

        public static Double GetAngleBetweenPointsRadians(Point point1, Point point2)
        {
            return (Math.Atan2(point1.Y - point2.Y, point2.X - point1.X));
        }

        public static Double RadiansToDegrees(Double radians)
        {
            return ((radians * 360.0) / (2.0 * Math.PI));
        }
    }
}
