using System.Windows;

namespace GraphX.WPF.Controls.Models
{
    /// <summary>
    /// Factory class responsible for VertexControl and EdgeControl objects creation
    /// </summary>
    public class GraphControlFactory : IGraphControlFactory
    {
        public GraphControlFactory(GraphAreaBase graphArea)
        {
            FactoryRootArea = graphArea;
        }

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
