using System.Windows;

namespace GraphX
{
    public interface IGraphControl
    {
        GraphAreaBase RootArea {get;set;}
        Point GetPosition(bool final = false, bool round = false);
        void SetPosition(Point pt, bool alsoFinal = true);
        void Clean();
    }
}