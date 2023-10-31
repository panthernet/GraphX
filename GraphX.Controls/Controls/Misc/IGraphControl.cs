using System.Windows;

namespace GraphX.Controls
{
    public interface IGraphControl : IPositionChangeNotify
    {
        GraphAreaBase RootArea { get; }
        Point GetPosition(bool final = false, bool round = false);
        void SetPosition(Point pt, bool alsoFinal = true);
        void SetPosition(double x, double y, bool alsoFinal = true);
        Visibility Visibility { get; set; }
        void Clean();
    }
}