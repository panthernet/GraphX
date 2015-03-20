using Windows.UI.Xaml;
using GraphX.Controls;

namespace GraphX.METRO.Controls.Models.Interfaces
{
    public interface IGraphControlFactory
    {
        EdgeControl CreateEdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true, Visibility visibility = Visibility.Visible);
        VertexControl CreateVertexControl(object vertexData);
        GraphAreaBase FactoryRootArea { set; get; }
        
    }

}
