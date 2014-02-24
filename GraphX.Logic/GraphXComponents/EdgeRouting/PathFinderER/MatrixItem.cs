using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GraphX
{
    public class MatrixItem
    {
        public Point Point;
        public bool IsIntersected;

        public int PlaceX;
        public int PlaceY;

        public int Weight { get { return IsIntersected ? 0 : 1; } }

        public MatrixItem(Point pt, bool inter, int placeX, int placeY)
        {
            Point = pt; IsIntersected = inter;
            PlaceX = placeX; PlaceY = placeY;
        }
    }
}
