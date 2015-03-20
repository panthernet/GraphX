using Windows.UI.Xaml;
using GraphX.METRO.Controls.Models.Interfaces;

namespace GraphX.Controls.Models
{
    public class GraphControlFactory : IGraphControlFactory
    {

        public EdgeControl CreateEdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true, Visibility visibility = Visibility.Visible)
        {
            var edgectrl = new EdgeControl(source, target, edge, showLabels, showArrows) { Visibility = visibility, RootArea = FactoryRootArea};

            return edgectrl;

        }

        public VertexControl CreateVertexControl(object vertexData)
        {
            return new VertexControl(vertexData) {RootArea = FactoryRootArea};
        }


        public GraphAreaBase FactoryRootArea { get; set; }
    }
}
