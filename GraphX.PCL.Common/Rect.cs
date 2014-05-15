using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphX.Measure
{
    /// <summary>
    /// Custom PCL implementation of Rect class
    /// </summary>
    public class Rect
    {
        internal double _x;
        internal double _y;
        public double X { get { return _x; } set { _x = value; } }
        public double Y { get { return _y; } set { _y = value; } }

        public double Left { get { return _x; } }
        public double Top { get { return _y; } }
        public double Bottom { get { if (this.IsEmpty) return double.NegativeInfinity; return (this._y + this._height); } }
        public double Right { get { if (this.IsEmpty) return double.NegativeInfinity; return (this._x + this._width); } }

        internal double _width;
        public double Width { get { return _width; } 
            set {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                if (value < 0.0)
                    throw new ArgumentException("Size_WidthCannotBeNegative");
                _width = value;
            }
        }
        internal double _height;
        public double Height { get { return _height; } 
            set {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                if (value < 0.0)
                    throw new ArgumentException("Size_HeightCannotBeNegative");
                _height = value;
            } }

        public Point BottomLeft { get { return new Point(_x, Bottom); } }
        public Point TopLeft { get { return new Point(_x, _y); } }
        public Point TopRight { get { return new Point(Right, _y); } }
        public Point BottomRight { get { return new Point(Right, Bottom); } }

        private static readonly Rect s_empty;
        public static Rect Empty { get { return s_empty; } }
        public bool IsEmpty { get { return (this._width < 0.0); } }

        public Point Location
        {
            get
            {
                return new Point(this._x, this._y);
            }
            set
            {
                if (this.IsEmpty)
                {
                    this._x = 0;
                    this._y = 0;
                    return;                    
                }
                this._x = value._x;
                this._y = value._y;
            }
        }
        public Size Size
        {
            get
            {
                if (this.IsEmpty)
                {
                    return Size.Empty;
                }
                return new Size(this._width, this._height);
            }
            set
            {
                if (value.IsEmpty)
                {
                    this._width = 0;
                    this._height = 0;
                }
                else
                {
                    if (this.IsEmpty)
                    {
                        this._width = 0;
                        this._height = 0;
                        return;
                    }
                    this._width = value._width;
                    this._height = value._height;
                }
            }
        }

        public Rect() { }

        public Rect(Point location, Size size)
        {
            if (size.IsEmpty)
            {
                this._x = 0;
                this._y = 0;
                this._width = 0;
                this._height = 0;
            }
            else
            {
                this._x = location._x;
                this._y = location._y;
                this._width = size._width;
                this._height = size._height;
            }
        }

        public Rect(double x, double y, double width, double height)
        {
            if ((width < 0.0) || (height < 0.0))
            {
                throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
            }
            this._x = x;
            this._y = y;
            this._width = width;
            this._height = height;
        }

        public Rect(Point point1, Point point2)
        {
            this._x = Math.Min(point1._x, point2._x);
            this._y = Math.Min(point1._y, point2._y);
            this._width = Math.Max((double)(Math.Max(point1._x, point2._x) - this._x), (double)0.0);
            this._height = Math.Max((double)(Math.Max(point1._y, point2._y) - this._y), (double)0.0);
        }

        public Rect(Point point, Vector vector)
            : this(point, point + vector)
        {
        }

        public Rect(Size size)
        {
            if (size.IsEmpty)
            {
                this._x = 0;
                this._y = 0;
                this._width = 0;
                this._height = 0;
            }
            else
            {
                this._x = this._y = 0.0;
                this._width = size.Width;
                this._height = size.Height;
            }
        }

        #region Custom operator overloads

        public static bool operator ==(Rect value1, Rect value2)
        {
            return value1.Left == value2.Left && value1.Top == value2.Top && value1.Right == value2.Right && value1.Bottom == value2.Bottom;
        }

        public static bool operator !=(Rect rect1, Rect rect2)
        {
            return !(rect1 == rect2);
        }

        #endregion

        public static bool Equals(Rect rect1, Rect rect2)
        {
            if (rect1.IsEmpty)
            {
                return rect2.IsEmpty;
            }
            return (((rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y)) && rect1.Width.Equals(rect2.Width)) && rect1.Height.Equals(rect2.Height));
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is Rect))
            {
                return false;
            }
            Rect rect = (Rect)o;
            return Equals(this, rect);
        }

        public bool Equals(Rect value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (this.IsEmpty)
            {
                return 0;
            }
            return (((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Width.GetHashCode()) ^ this.Height.GetHashCode());
        }

        public void Offset(Vector offsetVector)
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            this._x += offsetVector._x;
            this._y += offsetVector._y;
        }

        public void Offset(double offsetX, double offsetY)
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            this._x += offsetX;
            this._y += offsetY;
        }

        public static Rect Offset(Rect rect, Vector offsetVector)
        {
            rect.Offset(offsetVector.X, offsetVector.Y);
            return rect;
        }

        public static Rect Offset(Rect rect, double offsetX, double offsetY)
        {
            rect.Offset(offsetX, offsetY);
            return rect;
        }

        public bool IntersectsWith(Rect rect)
        {
            if (this.IsEmpty || rect.IsEmpty)
            {
                return false;
            }
            return ((((rect.Left <= this.Right) && (rect.Right >= this.Left)) && (rect.Top <= this.Bottom)) && (rect.Bottom >= this.Top));
        }

        public void Intersect(Rect rect)
        {
            if (!this.IntersectsWith(rect))
            {
                this._x = 0;
                this._y = 0;
                this._width = 0;
                this._height = 0;
            }
            else
            {
                double num2 = Math.Max(this.Left, rect.Left);
                double num = Math.Max(this.Top, rect.Top);
                this._width = Math.Max((double)(Math.Min(this.Right, rect.Right) - num2), (double)0.0);
                this._height = Math.Max((double)(Math.Min(this.Bottom, rect.Bottom) - num), (double)0.0);
                this._x = num2;
                this._y = num;
            }
        }

        public static Rect Intersect(Rect rect1, Rect rect2)
        {
            rect1.Intersect(rect2);
            return rect1;
        }

        public void Union(Rect rect)
        {
            if (this.IsEmpty)
            {
                this._x = 0;
                this._y = 0;
                this._width = 0;
                this._height = 0;
            }
            else if (!rect.IsEmpty)
            {
                double num2 = Math.Min(this.Left, rect.Left);
                double num = Math.Min(this.Top, rect.Top);
                if ((rect.Width == double.PositiveInfinity) || (this.Width == double.PositiveInfinity))
                {
                    this._width = double.PositiveInfinity;
                }
                else
                {
                    double num4 = Math.Max(this.Right, rect.Right);
                    this._width = Math.Max((double)(num4 - num2), (double)0.0);
                }
                if ((rect.Height == double.PositiveInfinity) || (this.Height == double.PositiveInfinity))
                {
                    this._height = double.PositiveInfinity;
                }
                else
                {
                    double num3 = Math.Max(this.Bottom, rect.Bottom);
                    this._height = Math.Max((double)(num3 - num), (double)0.0);
                }
                this._x = num2;
                this._y = num;
            }
        }

        public static Rect Union(Rect rect1, Rect rect2)
        {
            rect1.Union(rect2);
            return rect1;
        }

        public void Union(Point point)
        {
            this.Union(new Rect(point, point));
        }

        public static Rect Union(Rect rect, Point point)
        {
            rect.Union(new Rect(point, point));
            return rect;
        }

        public bool Contains(Point point)
        {
            return this.Contains(point._x, point._y);
        }

        public bool Contains(double x, double y)
        {
            if (this.IsEmpty)
            {
                return false;
            }
            return this.ContainsInternal(x, y);
        }

        public bool Contains(Rect rect)
        {
            if (this.IsEmpty || rect.IsEmpty)
            {
                return false;
            }
            return ((((this._x <= rect._x) && (this._y <= rect._y)) && ((this._x + this._width) >= (rect._x + rect._width))) && ((this._y + this._height) >= (rect._y + rect._height)));
        }

        private bool ContainsInternal(double x, double y)
        {
            return ((((x >= this._x) && ((x - this._width) <= this._x)) && (y >= this._y)) && ((y - this._height) <= this._y));
        }

        private static Rect CreateEmptyRect()
        {
            return new Rect() { _x = double.PositiveInfinity, _y = double.PositiveInfinity, _width = double.NegativeInfinity, _height = double.NegativeInfinity };
        }

        static Rect()
        {
            s_empty = CreateEmptyRect();
        }

        public void Inflate(Size size)
        {
            this.Inflate(size._width, size._height);
        }

        public void Inflate(double width, double height)
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            this._x -= width;
            this._y -= height;
            this._width += width;
            this._width += width;
            this._height += height;
            this._height += height;
            if ((this._width < 0.0) || (this._height < 0.0))
            {
                this._x = 0;
                this._y = 0;
                this._width = 0;
                this._height = 0;
            }
        }

        public static Rect Inflate(Rect rect, Size size)
        {
            rect.Inflate(size._width, size._height);
            return rect;
        }


        public static Rect InflateNew(Rect rect, double width, double height)
        {
            var r = new Rect(rect._x, rect._y, rect._width, rect._height);
            r.Inflate(width, height);
            return r;
        }


        public static Rect Inflate(Rect rect, double width, double height)
        {
            rect.Inflate(width, height);
            return rect;
        }
    }
}
