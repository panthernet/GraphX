using System;
using System.Collections.Generic;
#if METRO
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
#elif WPF
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
#endif

namespace GraphX.Controls
{
    public static class TypeExtensions
    {
        public static void SetDesiredFrameRate(this Timeline tl, int fps)
        {
#if WPF
            Timeline.SetDesiredFrameRate(tl, fps);
#endif
        }

#if METRO
        public static void SetCurrentValue(this FrameworkElement el, DependencyProperty p, object value)
        {
            el.SetValue(p, value);
        }

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

        public static bool IsInDesignMode(this FrameworkElement el)
        {
            return DesignMode.DesignModeEnabled;
        }

        public static Point Offset(this Point pt, double offsetX, double offsetY)
        {
            pt.X += offsetX;
            pt.Y += offsetY;
            return pt;
        }

#elif WPF
        public static bool IsInDesignMode(this FrameworkElement el)
        {
            return DesignerProperties.GetIsInDesignMode(el);
        }

        public static void Offset(this Point point, Point value)
        {
            point.X = point.X + value.X;
            point.Y = point.Y + value.Y;
        }

        /// <summary>
        /// Not for METRO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindLogicalChildren<T>(this DependencyObject obj) 
            where T : DependencyObject
        {
            if (obj == null) yield break;
            var child = obj as T;
            if (child != null) yield return child;

            foreach (var c in LogicalTreeHelper.GetChildren(obj)
                                               .OfType<DependencyObject>()
                                               .SelectMany(FindLogicalChildren<T>))
                yield return c;
        }
#endif

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

        public static Measure.Point[] ToGraphX(this Point[] points)
        {
            if (points == null) return null;
            var list = new Measure.Point[points.Length];
            for (int i = 0; i < points.Length; i++)
                list[i] = points[i].ToGraphX();
            return list;
        }

        public static Size Size(this Rect rect)
        {
            return new Size(rect.Width, rect.Height);
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
    }
}
