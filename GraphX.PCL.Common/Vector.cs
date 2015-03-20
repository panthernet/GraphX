using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public Vector(double x, double y) { _x = x; _y = y; }

        private static readonly Vector zeroVector = new Vector();
        public static Vector Zero
        {
            get { return zeroVector; }
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
            return (Vector)(vector * (1.0 / scalar));
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
            if ((o == null) || !(o is Vector))
            {
                return false;
            }
            Vector vector = (Vector)o;
            return Equals(this, vector);
        }

        public bool Equals(Vector value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            return (this.X.GetHashCode() ^ this.Y.GetHashCode());
        }

        public double Length
        {
            get
            {
                return Math.Sqrt((this._x * this._x) + (this._y * this._y));
            }
        }
        public double LengthSquared
        {
            get
            {
                return ((this._x * this._x) + (this._y * this._y));
            }
        }
        public void Normalize()
        {
            var v = (Vector)(this / Math.Max(Math.Abs(this._x), Math.Abs(this._y)));
            v = (Vector)(this / this.Length);
            this._x = v._x;
            this._y = v._y;
        }

        public static double CrossProduct(Vector vector1, Vector vector2)
        {
            return ((vector1._x * vector2._y) - (vector1._y * vector2._x));
        }

        public static double AngleBetween(Vector vector1, Vector vector2)
        {
            double y = (vector1._x * vector2._y) - (vector2._x * vector1._y);
            double x = (vector1._x * vector2._x) + (vector1._y * vector2._y);
            return (Math.Atan2(y, x) * 57.295779513082323);
        }

        public void Negate()
        {
            this._x = -this._x;
            this._y = -this._y;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", _x, _y);
        }
    }
}
