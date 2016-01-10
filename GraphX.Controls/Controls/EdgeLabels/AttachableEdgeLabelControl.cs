

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

        
#if WPF
        static AttachableEdgeLabelControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AttachableEdgeLabelControl), new FrameworkPropertyMetadata(typeof(AttachableEdgeLabelControl)));
        }
#endif


        public AttachableEdgeLabelControl()
        {
            DataContext = this;
#if METRO
            DefaultStyleKey = typeof(AttachableEdgeLabelControl);
#endif
        }

        /// <summary>
        /// Attach label to VertexControl
        /// </summary>
        /// <param name="node">VertexControl node</param>
        public virtual void Attach(EdgeControl node)
        {
#if WPF
            if(AttachNode != null)
                AttachNode.IsVisibleChanged -= AttachNode_IsVisibleChanged;
            AttachNode = node;
            AttachNode.IsVisibleChanged += AttachNode_IsVisibleChanged;
#elif METRO
            AttachNode = node;
#endif
            node.AttachLabel(this);
        }

        /// <summary>
        /// Detach label from control
        /// </summary>
        public virtual void Detach()
        {
#if WPF
            if (AttachNode != null)
                AttachNode.IsVisibleChanged -= AttachNode_IsVisibleChanged;
#endif
            AttachNode = null;
        }

#if WPF
        void AttachNode_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(AttachNode.IsVisible && AttachNode.ShowLabel)
                base.Show();
            else if (!AttachNode.IsVisible)
            {
                base.Hide();
            }
        }
#endif
        protected override EdgeControl GetEdgeControl(DependencyObject parent)
        {
            if(AttachNode == null)
                throw new GX_InvalidDataException("AttachableEdgeLabelControl node is not attached!");
            return AttachNode;
        }
    }
}
