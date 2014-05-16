using System;
using System.Windows;

namespace GraphX
{
    public static class MathHelper
    {
        private const int LEFT  = 1;  /* двоичное 0001 */
        private const int RIGHT = 2;  /* двоичное 0010 */
        private const int BOT   = 4;  /* двоичное 0100 */
        private const int TOP   = 8;  /* двоичное 1000 */

        const double D30_DEGREES_IN_RADIANS = Math.PI / 6.0;

        public static double Tangent30Degrees { get; private set; }

        static MathHelper()
        {
            Tangent30Degrees = Math.Tan(D30_DEGREES_IN_RADIANS);
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
            /* код конечных точек отрезка */
            var codeA = GetIntersectionData(r, a);
            var codeB = GetIntersectionData(r, b);

            if (codeA.IsInside() && codeB.IsInside())
                return true;

            /* пока одна из точек отрезка вне прямоугольника */
            while (!codeA.IsInside() || !codeB.IsInside())
            {
                /* если обе точки с одной стороны прямоугольника, то отрезок не пересекает прямоугольник */
                if (codeA.SameSide(codeB))
                    return false;

                /* выбираем точку c с ненулевым кодом */
                sides code;
                Point c; /* одна из точек */
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

                /* если c левее r, то передвигаем c на прямую x = r->x_min
                   если c правее r, то передвигаем c на прямую x = r->x_max */
                if (code.Left)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if (code.Right)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }/* если c ниже r, то передвигаем c на прямую y = r->y_min
                    если c выше r, то передвигаем c на прямую y = r->y_max */
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

                /* обновляем код */
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
            sides code; 
            Point c; /* одна из точек */
            var start = new Point(a.X, a.Y);
            /* код конечных точек отрезка */
            var code_a = GetIntersectionData(r, a);
            var code_b = GetIntersectionData(r, b);

            /* пока одна из точек отрезка вне прямоугольника */
            while (!code_a.IsInside() || !code_b.IsInside())
            {
                /* если обе точки с одной стороны прямоугольника, то отрезок не пересекает прямоугольник */
                if (code_a.SameSide(code_b))
                {
                    pt = new Point();
                    return -1;
                }

                /* выбираем точку c с ненулевым кодом */
                if (!code_a.IsInside())
                {
                    code = code_a;
                    c = a;
                }
                else
                {
                    code = code_b;
                    c = b;
                }

                /* если c левее r, то передвигаем c на прямую x = r->x_min
                   если c правее r, то передвигаем c на прямую x = r->x_max */
                if (code.Left)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if (code.Right)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }/* если c ниже r, то передвигаем c на прямую y = r->y_min
                    если c выше r, то передвигаем c на прямую y = r->y_max */
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

                /* обновляем код */
                if (code == code_a)
                {
                    a = c;
                    code_a = GetIntersectionData(r, a);
                }
                else
                {
                    b = c;
                    code_b = GetIntersectionData(r, b);
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
                return (Left ==true && o.Left == true) || (Right == true && o.Right == true) || (Top == true && o.Top == true)
                    || (Bottom == true && o.Bottom == true);
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
