using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphX.Measure
{
    /// <summary>
    /// Custom PCL implementation of Point class
    /// </summary>
    public class Point
    {
        internal double _x;
        internal double _y;
        public double X
        {
            get
            {
                return this._x;
            }
            set
            {
                this._x = value;
            }
        }
        public double Y
        {
            get
            {
                return this._y;
            }
            set
            {
                this._y = value;
            }
        }

        public Point()
        {
        }

        public Point(double x, double y)
        {
            _x = x;
            _y = y;
        }

        #region Custom operator overloads
        public static bool operator ==(Point value1, Point value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }

        public static bool operator !=(Point value1, Point value2)
        {
            return value1 != value2;
        }

        ///// OTHER CLASSES CONVERSIONS

        public static implicit operator Point(Size size)
        {
            return new Point(size.Width, size.Height);
        }

        public static implicit operator Point(Vector size)
        {
            return new Point(size.X, size.Y);
        }

        public static explicit operator Size(Point point)
        {
            return new Size(Math.Abs(point._x), Math.Abs(point._y));
        }

        public static explicit operator Vector(Point point)
        {
            return new Vector(point._x, point._y);
        }

        ///// OTHER CLASSES ARITHM + CONVERSIONS

        public static Point operator +(Point point, Vector vector)
        {
            return new Point(point._x + vector._x, point._y + vector._y);
        }

        public static Point operator -(Point point, Vector vector)
        {
            return new Point(point._x - vector._x, point._y - vector._y);
        }

        /// ARITHMETIC

        public static Vector operator -(Point value1, Point value2)
        {
            return new Vector(value1._x - value2._x, value1._y - value2._y);
        }

        public static Point operator +(Point value1, Point value2)
        {
            return new Point(value1._x + value2._x, value1._y + value2._y);
        }

        public static Point operator *(double value1, Point value2)
        {
            return new Point(value1 * value2.X, value1 * value2.Y);
        }

        public static Point operator *(Point value1, double value2)
        {
            return new Point(value1.X * value2, value1.Y * value2);
        }

        public static Point operator /(Point value1, double value2)
        {
            return new Point(value1.X * value2, value1.Y * value2);
        }

        

        public static bool Equals(Point point1, Point point2)
        {
            return (point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y));
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is Point))
            {
                return false;
            }
            Point point = (Point)o;
            return Equals(this, point);
        }

        public bool Equals(Point value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            return (this.X.GetHashCode() ^ this.Y.GetHashCode());
        }

        public void Offset(double offsetX, double offsetY)
        {
            this._x += offsetX;
            this._y += offsetY;
        }

        #endregion
    }
}
