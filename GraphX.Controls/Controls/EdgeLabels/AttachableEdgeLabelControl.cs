

using GraphX.Controls.Models;
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
    public class AttachableEdgeLabelControl : EdgeLabelControl, IAttachableControl<EdgeControl>
    {
        /// <summary>
        /// Gets label attach node
        /// </summary>
        public EdgeControl AttachNode { get { return (EdgeControl) GetValue(AttachNodeProperty); } private set {SetValue(AttachNodeProperty, value);} }

        public static readonly DependencyProperty AttachNodeProperty = DependencyProperty.Register("AttachNode", typeof(EdgeControl), typeof(AttachableEdgeLabelControl), 
            new PropertyMetadata(null));


        public AttachableEdgeLabelControl()
        {
            DataContext = this;
        }

        /// <summary>
        /// Attach label to VertexControl
        /// </summary>
        /// <param name="node">VertexControl node</param>
        public void Attach(EdgeControl node)
        {
            AttachNode = node;
            node.AttachEdgeLabel(this);
        }

        protected override EdgeControl GetEdgeControl(DependencyObject parent)
        {
            if(AttachNode == null)
                throw new GX_InvalidDataException("AttachableEdgeLabelControl node is not attached!");
            return AttachNode;
        }
    }
}
