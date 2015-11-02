
#if WPF
using System.Windows;
using System.Windows.Controls;
using DefaultEventArgs = System.EventArgs;
#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using DefaultEventArgs = System.Object;
#endif
using GraphX.PCL.Common.Exceptions;

namespace GraphX.Controls
{
#if METRO
    [Bindable]
#endif
    public class AttachableEdgeLabelControl : EdgeLabelControl
    {
        public EdgeControl AttachNode { get { return (EdgeControl) GetValue(AttachNodeProperty); } private set {SetValue(AttachNodeProperty, value);} }

        public static readonly DependencyProperty AttachNodeProperty = DependencyProperty.Register("AttachNode", typeof(EdgeControl), typeof(AttachableEdgeLabelControl), 
            new PropertyMetadata(null));


        public AttachableEdgeLabelControl()
        {
            DataContext = this;
        }

        public void Attach(EdgeControl node)
        {
            AttachNode = node;
            node.InjectEdgeLable(this);
        }

        protected override EdgeControl GetEdgeControl(DependencyObject parent)
        {
            if(AttachNode == null)
                throw new GX_InvalidDataException("AttachableEdgeLabelControl node is not attached!");
            return AttachNode;
        }

       /* public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
           // var container = Template.FindName("PART_container", this) as ContentPresenter;
            //container.Content = AttachNode.Vertex;
        }*/
    }
}
