using System;

namespace GraphX.Measure
{
    public struct Vector
    {
        internal double _x;
        internal double _y;
        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }
        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public Vector(double x, double y) { _x = x; _y = y; }

        private static readonly Vector ZeroVector = new Vector();
        public static Vector Zero
        {
            get { return ZeroVector; }
        }

        #region Overloaded operators

        public static bool operator ==(Vector vector1, Vector vector2)
        {
            return vector1.X == vector2.X && vector1.Y == vector2.Y;
        }

        public static bool operator !=(Vector vector1, Vector vector2)
        {
            return !(vector1 == vector2);
        }



        public static double operator *(Vector vector1, Vector vector2)
        {
            return ((vector1._x * vector2._x) + (vector1._y * vector2._y));
        }

        public static Vector operator *(double scalar, Vector vector)
        {
            return new Vector(vector._x * scalar, vector._y * scalar);
        }

        public static Vector operator *(Vector vector, double scalar)
        {
            return new Vector(vector._x * scalar, vector._y * scalar);
        }

        public static Vector operator *(int value1, Vector value2)
        {
            return new Vector(value1 * value2.X, value1 * value2.Y);
        }

        public static Vector operator +(Vector value1, Vector value2)
        {
            return new Vector(value1.X + value2.X, value1.Y + value2.Y);        
        }

        public static Vector operator -(Vector value1, Vector value2)
        {
            return new Vector(value1.X - value2.X, value1.Y - value2.Y);
        }

        public static Vector operator /(Vector vector, double scalar)
        {
            return vector * (1.0 / scalar);
        }

        public static Vector operator -(Vector value1)
        {
            return new Vector(-value1.X, -value1.Y);
        }



        
        public static Point operator +(Vector value1, Point value2)
        {
            return new Point(value1.X + value2.X, value1.Y + value2.Y);
        }

       /* public static Vector operator /(Vector value1, Vector value2)
        {
            return new Vector(value1.X / value2.X, value1.Y / value2.Y);
        }*/

        public static Vector operator -(Vector value1, Point value2)
        {
            return new Vector(value1.X - value2.X, value1.Y - value2.Y);
        }


        #endregion

        public static bool Equals(Vector vector1, Vector vector2)
        {
            return (vector1.X.Equals(vector2.X) && vector1.Y.Equals(vector2.Y));
        }

        public override bool Equals(object o)
        {
            if (!(o is Vector))
                return false;
            return Equals(this, (Vector)o);
        }

        public bool Equals(Vector value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode());
        }

        public double Length
        {
            get
            {
                return Math.Sqrt((_x * _x) + (_y * _y));
            }
        }
        public double LengthSquared
        {
            get
            {
                return ((_x * _x) + (_y * _y));
            }
        }
        public void Normalize()
        {
            var v = this / Math.Max(Math.Abs(_x), Math.Abs(_y));
            v = this / Length;
            _x = v._x;
            _y = v._y;
        }

        public static double CrossProduct(Vector vector1, Vector vector2)
        {
            return ((vector1._x * vector2._y) - (vector1._y * vector2._x));
        }

        public static double AngleBetween(Vector vector1, Vector vector2)
        {
            var y = (vector1._x * vector2._y) - (vector2._x * vector1._y);
            var x = (vector1._x * vector2._x) + (vector1._y * vector2._y);
            return (Math.Atan2(y, x) * 57.295779513082323);
        }

        public void Negate()
        {
            _x = -_x;
            _y = -_y;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", _x, _y);
        }
    }
}
