using System;

namespace GraphX.Measure
{
    /// <summary>
    /// Custom PCL implementation of Rect class
    /// </summary>
    public struct Rect
    {
        internal double _x;
        internal double _y;
        public double X { get { return _x; } set { _x = value; } }
        public double Y { get { return _y; } set { _y = value; } }

        public double Left { get { return _x; } }
        public double Top { get { return _y; } }
        public double Bottom { get { if (IsEmpty) return double.NegativeInfinity; return (_y + _height); } }
        public double Right { get { if (IsEmpty) return double.NegativeInfinity; return (_x + _width); } }

        internal double _width;
        public double Width { get { return _width; } 
            set {
                if (IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                if (value < 0.0)
                    throw new ArgumentException("Size_WidthCannotBeNegative");
                _width = value;
            }
        }
        internal double _height;
        public double Height { get { return _height; } 
            set {
                if (IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                if (value < 0.0)
                    throw new ArgumentException("Size_HeightCannotBeNegative");
                _height = value;
            } }

        public Point BottomLeft { get { return new Point(_x, Bottom); } }
        public Point TopLeft { get { return new Point(_x, _y); } }
        public Point TopRight { get { return new Point(Right, _y); } }
        public Point BottomRight { get { return new Point(Right, Bottom); } }

        private static readonly Rect SEmpty;
        public static Rect Empty { get { return SEmpty; } }
        public bool IsEmpty { get { return (_width < 0.0); } }

        public Point Location
        {
            get
            {
                return new Point(_x, _y);
            }
            set
            {
                if (IsEmpty)
                {
                    _x = 0;
                    _y = 0;
                    return;                    
                }
                _x = value._x;
                _y = value._y;
            }
        }
        public Size Size
        {
            get
            {
                if (IsEmpty)
                {
                    return Size.Empty;
                }
                return new Size(_width, _height);
            }
            set
            {
                if (value.IsEmpty)
                {
                    _width = 0;
                    _height = 0;
                }
                else
                {
                    if (IsEmpty)
                    {
                        _width = 0;
                        _height = 0;
                        return;
                    }
                    _width = value._width;
                    _height = value._height;
                }
            }
        }


        public Rect(Point location, Size size)
        {
            if (size.IsEmpty)
            {
                _x = 0;
                _y = 0;
                _width = 0;
                _height = 0;
            }
            else
            {
                _x = location._x;
                _y = location._y;
                _width = size._width;
                _height = size._height;
            }
        }

        public Rect(double x, double y, double width, double height)
        {
            if ((width < 0.0) || (height < 0.0))
            {
                throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
            }
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public Rect(Point point1, Point point2)
        {
            _x = Math.Min(point1._x, point2._x);
            _y = Math.Min(point1._y, point2._y);
            _width = Math.Max(Math.Max(point1._x, point2._x) - _x, 0);
            _height = Math.Max(Math.Max(point1._y, point2._y) - _y, 0);
        }

        public Rect(Point point, Vector vector)
            : this(point, point + vector)
        {
        }

        public Rect(Size size)
        {
            if (size.IsEmpty)
            {
                _x = 0;
                _y = 0;
                _width = 0;
                _height = 0;
            }
            else
            {
                _x = _y = 0.0;
                _width = size.Width;
                _height = size.Height;
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
            if (!(o is Rect))
                return false;
            return Equals(this, (Rect)o);
        }

        public bool Equals(Rect value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
            {
                return 0;
            }
            return (((X.GetHashCode() ^ Y.GetHashCode()) ^ Width.GetHashCode()) ^ Height.GetHashCode());
        }

        public void Offset(Vector offsetVector)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            _x += offsetVector._x;
            _y += offsetVector._y;
        }

        public void Offset(double offsetX, double offsetY)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            _x += offsetX;
            _y += offsetY;
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
            if (IsEmpty || rect.IsEmpty)
            {
                return false;
            }
            return ((((rect.Left <= Right) && (rect.Right >= Left)) && (rect.Top <= Bottom)) && (rect.Bottom >= Top));
        }

        public void Intersect(Rect rect)
        {
            if (!IntersectsWith(rect))
            {
                _x = 0;
                _y = 0;
                _width = 0;
                _height = 0;
            }
            else
            {
                var num2 = Math.Max(Left, rect.Left);
                var num = Math.Max(Top, rect.Top);
                _width = Math.Max(Math.Min(Right, rect.Right) - num2, 0.0);
                _height = Math.Max(Math.Min(Bottom, rect.Bottom) - num, 0.0);
                _x = num2;
                _y = num;
            }
        }

        public static Rect Intersect(Rect rect1, Rect rect2)
        {
            rect1.Intersect(rect2);
            return rect1;
        }

        public void Union(Rect rect)
        {
            if (IsEmpty)
            {
                _x = 0;
                _y = 0;
                _width = 0;
                _height = 0;
            }
            else if (!rect.IsEmpty)
            {
                double num2 = Math.Min(Left, rect.Left);
                double num = Math.Min(Top, rect.Top);
                if ((rect.Width == double.PositiveInfinity) || (Width == double.PositiveInfinity))
                {
                    _width = double.PositiveInfinity;
                }
                else
                {
                    double num4 = Math.Max(Right, rect.Right);
                    _width = Math.Max(num4 - num2, 0.0);
                }
                if ((rect.Height == double.PositiveInfinity) || (Height == double.PositiveInfinity))
                {
                    _height = double.PositiveInfinity;
                }
                else
                {
                    double num3 = Math.Max(Bottom, rect.Bottom);
                    _height = Math.Max(num3 - num, 0.0);
                }
                _x = num2;
                _y = num;
            }
        }

        public static Rect Union(Rect rect1, Rect rect2)
        {
            rect1.Union(rect2);
            return rect1;
        }

        public void Union(Point point)
        {
            Union(new Rect(point, point));
        }

        public static Rect Union(Rect rect, Point point)
        {
            rect.Union(new Rect(point, point));
            return rect;
        }

        public bool Contains(Point point)
        {
            return Contains(point._x, point._y);
        }

        public bool Contains(double x, double y)
        {
            if (IsEmpty)
            {
                return false;
            }
            return ContainsInternal(x, y);
        }

        public bool Contains(Rect rect)
        {
            if (IsEmpty || rect.IsEmpty)
            {
                return false;
            }
            return ((((_x <= rect._x) && (_y <= rect._y)) && ((_x + _width) >= (rect._x + rect._width))) && ((_y + _height) >= (rect._y + rect._height)));
        }

        private bool ContainsInternal(double x, double y)
        {
            return ((((x >= _x) && ((x - _width) <= _x)) && (y >= _y)) && ((y - _height) <= _y));
        }

        private static Rect CreateEmptyRect()
        {
            return new Rect() { _x = double.PositiveInfinity, _y = double.PositiveInfinity, _width = double.NegativeInfinity, _height = double.NegativeInfinity };
        }

        static Rect()
        {
            SEmpty = CreateEmptyRect();
        }

        public void Inflate(Size size)
        {
            Inflate(size._width, size._height);
        }

        public void Inflate(double width, double height)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            _x -= width;
            _y -= height;
            _width += width;
            _width += width;
            _height += height;
            _height += height;
            if ((_width < 0.0) || (_height < 0.0))
            {
                _x = 0;
                _y = 0;
                _width = 0;
                _height = 0;
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
