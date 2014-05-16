using System.Windows;
namespace GraphX
{
    public interface IGraphControl
    {
        GraphAreaBase RootArea {get;set;}
        Point GetPosition(bool final = false, bool round = false);
        //Measure.Point GetPositionGraphX(bool final = false, bool round = false);
        void SetPosition(Point pt, bool alsoFinal = true);
        void SetPosition(double x, double y, bool alsoFinal = true);
        void Clean();
    }
}