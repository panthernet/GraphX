using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphX.Measure
{
    public class Size
    {
        internal double _width;
        public double Width { get { return this._width; } 
            set 
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Size_CannotModifyEmptySize");
                if (value < 0.0)
                    throw new ArgumentException("Size_WidthCannotBeNegative");
                this._width = value; 
            } }
        internal double _height;
        public double Height { get { return this._height; } 
            set 
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Size_CannotModifyEmptySize");
                if (value < 0.0)
                    throw new ArgumentException("Size_HeightCannotBeNegative");
                this._width = value;
                this._height = value; 
            } }

        private static readonly Size s_empty;
        public static Size Empty { get { return s_empty; } }
        public bool IsEmpty { get { return (this._width < 0.0); } }

        public Size() { }
        public Size(double width, double height) 
        {
            if ((width < 0.0) || (height < 0.0))
            {
                throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
            } 
            this._width = width; this._height = height; 
        }

        #region Custom operators overload

        public static bool operator ==(Size size1, Size size2)
        {
            return ((size1.Width == size2.Width) && (size1.Height == size2.Height));
        }

        public static bool operator !=(Size size1, Size size2)
        {
            return !(size1 == size2);
        }

        public static explicit operator Vector(Size size)
        {
            return new Vector(size._width, size._height);
        }

        public static explicit operator Point(Size size)
        {
            return new Point(size._width, size._height);
        }

        #endregion

        private static Size CreateEmptySize()
        {
            return new Size { _width = double.NegativeInfinity, _height = double.NegativeInfinity };
        }

        static Size()
        {
            s_empty = CreateEmptySize();
        }

        public static bool Equals(Size size1, Size size2)
        {
            if (size1.IsEmpty)
            {
                return size2.IsEmpty;
            }
            return (size1.Width.Equals(size2.Width) && size1.Height.Equals(size2.Height));
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is Size))
            {
                return false;
            }
            Size size = (Size)o;
            return Equals(this, size);
        }

        public bool Equals(Size value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (this.IsEmpty)
            {
                return 0;
            }
            return (this.Width.GetHashCode() ^ this.Height.GetHashCode());
        }
    }
}
