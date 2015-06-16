using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace GraphX.Controls
{
    public static class TypeExtensions
    {

        public static void RotateAt(this Matrix imatrix, double angle, double centerX, double centerY)
        {
            angle %= 360.0;
            var matrix = new Matrix();
            double m12 = Math.Sin(angle * (Math.PI / 180.0));
            double num = Math.Cos(angle * (Math.PI / 180.0));
            double offsetX = centerX * (1.0 - num) + centerY * m12;
            double offsetY = centerY * (1.0 - num) - centerX * m12;
            matrix.M12 = m12;
            matrix.OffsetX = offsetX;
            matrix.OffsetY = offsetY;
            //!!!!!TODO
            //!!!imatrix *= matrix;
            imatrix = matrix;
        }

        public static Point ToWindows(this Measure.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point ToWindows(this Measure.Vector point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point Subtract(this Point pt, Point pt2)
        {
            return new Point(pt.X - pt2.X, pt.Y - pt2.Y);
        }


        public static Point Div(this Point pt, double value)
        {
            return new Point(pt.X / value, pt.Y / value);
        }

        public static Point Mul(this Point pt, double value)
        {
            return new Point(pt.X * value, pt.Y * value);
        }

        public static Point Sum(this Point pt, Point pt2)
        {
            return new Point(pt.X + pt2.X, pt.Y + pt2.Y);
        }

        public static Point TopLeft(this Rect rect)
        {
            return new Point(rect.Top, rect.Left);
        }

        public static Point BottomRight(this Rect rect)
        {
            return new Point(rect.Bottom, rect.Right);
        }

        public static Size Size(this Rect rect)
        {
            return new Size(rect.Width, rect.Height);
        }

        public static Point Offset(this Point pt, double offsetX, double offsetY)
        {
            pt.X += offsetX;
            pt.Y += offsetY;
            return pt;
        }

        public static PointCollection ToPointCollection(this IEnumerable<Point> points)
        {
            var list = new PointCollection();
            foreach (var item in points)
                list.Add(item);
            return list;
        }


        public static Point[] ToWindows(this Measure.Point[] points)
        {
            if (points == null) return null;
            var list = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
                list[i] = points[i].ToWindows();
            return list;
        }

        public static Measure.Point ToGraphX(this Point point)
        {
            return new Measure.Point(point.X, point.Y);
        }

        public static Measure.Size ToGraphX(this Size point)
        {
            return new Measure.Size(point.Width, point.Height);
        }

        public static Measure.Rect ToGraphX(this Rect rect)
        {
            return new Measure.Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Rect ToWindows(this Measure.Rect rect)
        {
            return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public static Point Center(this Rect rect)
        {
            return new Point(rect.X + rect.Width * .5, rect.Y + rect.Height * .5);
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> func)
        {
            foreach (var item in list)
                func(item);
        }
    }
}
