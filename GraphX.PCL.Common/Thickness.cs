using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphX.Measure
{
    public class Thickness
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Right { get; set; }

        public Thickness() { }
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
    }
}
