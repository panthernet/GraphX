# if WPF
using System.Windows;
#elif METRO
using Windows.UI.Xaml;
#endif

namespace GraphX.Controls.Models
{
    public interface IGraphControlFactory
    {
        EdgeControl CreateEdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true, Visibility visibility = Visibility.Visible);
        VertexControl CreateVertexControl(object vertexData);
        /// <summary>
        /// Root graph area for the factory
        /// </summary>
        GraphAreaBase FactoryRootArea { get; }
        
    }

}
