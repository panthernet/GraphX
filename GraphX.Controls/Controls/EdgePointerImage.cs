using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphX
{
    public class EdgePointerImage: Image
    {
        internal Rect LastKnownRectSize;

        public static readonly DependencyProperty NeedRotationProperty = DependencyProperty.Register("NeedRotation",
                                                                                       typeof(bool),
                                                                                       typeof(EdgeControl),
                                                                                       new UIPropertyMetadata(true));
        /// <summary>
        /// Gets or sets if image has to be rotated according to edge directions
        /// </summary>
        public bool NeedRotation
        {
            get { return (bool)GetValue(NeedRotationProperty); }
            set { SetValue(NeedRotationProperty, value); }
        }

       /* public static readonly DependencyProperty EdgePointerTypeProperty = DependencyProperty.Register("EdgePointerType",
                                                                               typeof(EdgePointerType),
                                                                               typeof(EdgeControl),
                                                                               new UIPropertyMetadata());
        /// <summary>
        /// Gets or sets edge pointer type: From or To
        /// </summary>
        public EdgePointerType EdgePointerType
        {
            get { return (EdgePointerType)GetValue(EdgePointerTypeProperty); }
            set { SetValue(EdgePointerTypeProperty, value); }
        }*/

        public EdgePointerImage()
        {
            RenderTransformOrigin = new Point(.5, .5);
            LayoutUpdated += EdgePointerImage_LayoutUpdated;
        }

        private static EdgeControl GetEdgeControl(DependencyObject parent)
        {
            while (parent != null)
            {
                var control = parent as EdgeControl;
                if (control != null) return control;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        void EdgePointerImage_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (LastKnownRectSize == Rect.Empty || double.IsNaN(LastKnownRectSize.Width) || LastKnownRectSize.Width == 0)
            {
                UpdateLayout();
                var ctrl = GetEdgeControl(VisualParent);
                if (ctrl != null && !ctrl.IsSelfLooped)
                    Update(this.Name.EndsWith("Source") ? ctrl.SourceConnectionPoint : ctrl.TargetConnectionPoint);
            }
            else Arrange(LastKnownRectSize);
        }

        public void Update(Point? position, double angle = 0d)
        {
            if (position.HasValue && DesiredSize != Size.Empty)
            {
                LastKnownRectSize = new Rect(new Point(position.Value.X - DesiredSize.Width * .5, position.Value.Y - DesiredSize.Height * .5), DesiredSize);
                Arrange(LastKnownRectSize);
            }else if (position == null)

            if(!NeedRotation) return;
            
            RenderTransform = new RotateTransform(angle, .5,.5);
        }

    }

    public enum EdgePointerType
    {
        Start = 0,
        End
    }
}
