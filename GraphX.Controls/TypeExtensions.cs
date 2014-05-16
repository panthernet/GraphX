using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace GraphX
{
    public static class TypeExtensions
    {
        public static Point ToWindows(this Measure.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point ToWindows(this Measure.Vector point)
        {
            return new Point(point.X, point.Y);
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
    }
}
