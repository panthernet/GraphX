using System.Windows;
using Windows.Foundation;

namespace GraphX
{
    public interface IGraphControl : IPositionChangeNotify
    {
        GraphAreaBase RootArea {get; }
        Point GetPosition(bool final = false, bool round = false);
        void SetPosition(Point pt, bool alsoFinal = true);
        void SetPosition(double x, double y, bool alsoFinal = true);
        void Clean();
    }
}