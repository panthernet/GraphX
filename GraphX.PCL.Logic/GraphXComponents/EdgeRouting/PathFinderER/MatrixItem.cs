using GraphX.Measure;

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
