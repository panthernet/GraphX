

using GraphX.Controls.Models;
using System.Windows;
using System.Windows.Controls;
using DefaultEventArgs = System.EventArgs;
using GraphX.Common.Exceptions;

namespace GraphX.Controls
{
    public class AttachableEdgeLabelControl : EdgeLabelControl, IAttachableControl<EdgeControl>
    {
        /// <summary>
        /// Gets label attach node
        /// </summary>
        public EdgeControl? AttachNode { get { return (EdgeControl) GetValue(AttachNodeProperty); } private set {SetValue(AttachNodeProperty, value);} }

        public static readonly DependencyProperty AttachNodeProperty = DependencyProperty.Register(nameof(AttachNode), typeof(EdgeControl), typeof(AttachableEdgeLabelControl), 
            new PropertyMetadata(null));

        static AttachableEdgeLabelControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AttachableEdgeLabelControl), new FrameworkPropertyMetadata(typeof(AttachableEdgeLabelControl)));
        }

        public AttachableEdgeLabelControl()
        {
            DataContext = this;
        }

        /// <summary>
        /// Attach label to VertexControl
        /// </summary>
        /// <param name="node">VertexControl node</param>
        public virtual void Attach(EdgeControl node)
        {
            if(AttachNode != null)
                AttachNode.IsVisibleChanged -= AttachNode_IsVisibleChanged;
            AttachNode = node;
            AttachNode.IsVisibleChanged += AttachNode_IsVisibleChanged;
            node.AttachLabel(this);
        }

        /// <summary>
        /// Detach label from control
        /// </summary>
        public virtual void Detach()
        {
            if (AttachNode != null)
                AttachNode.IsVisibleChanged -= AttachNode_IsVisibleChanged;
            AttachNode = null;
        }

        void AttachNode_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(AttachNode!.IsVisible && ShowLabel)
                base.Show();
            else if (!AttachNode.IsVisible)
            {
                base.Hide();
            }
        }

        protected override EdgeControl GetEdgeControl(DependencyObject? parent)
        {
            if(AttachNode == null)
                throw new GX_InvalidDataException("AttachableEdgeLabelControl node is not attached!");
            return AttachNode;
        }
    }
}
