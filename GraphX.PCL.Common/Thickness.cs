using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphX.Measure
{
    public struct Thickness
    {
        public double Left;
        public double Top;
        public double Bottom;
        public double Right;

        public Thickness(double left, double top, double right, double bottom)
        {
            Left = left; Right = right;
            Top = top; Bottom = bottom;
        }

        public static bool operator !=(Thickness t1, Thickness t2)
        {
            return !(t1.Left == t2.Left && t1.Top == t2.Top && t1.Right == t2.Right && t1.Bottom == t2.Bottom);
        }

        public static bool operator ==(Thickness t1, Thickness t2)
        {
            return t1.Left == t2.Left && t1.Top == t2.Top && t1.Right == t2.Right && t1.Bottom == t2.Bottom;
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is Thickness))
            {
                return false;
            }
            var vector = (Thickness)o;
            return Equals(this, vector);
        }

        public bool Equals(Thickness value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            return (Left.GetHashCode() ^ Top.GetHashCode() ^ Right.GetHashCode() ^ Bottom.GetHashCode());
        }
    }
}
