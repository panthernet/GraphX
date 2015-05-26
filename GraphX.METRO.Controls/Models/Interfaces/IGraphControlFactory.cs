using Windows.UI.Xaml;

namespace GraphX.Controls.Models
{
    public interface IGraphControlFactory
    {
        EdgeControl CreateEdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true, Visibility visibility = Visibility.Visible);
        VertexControl CreateVertexControl(object vertexData);
        GraphAreaBase FactoryRootArea { set; get; }
        
    }

}
