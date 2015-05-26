using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphX.Controls
{
    public class EdgePointerImage: Image, IEdgePointer
    {
        #region Common part
        internal Rect LastKnownRectSize;


        /*public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register("Offset",
                                                                               typeof(Point),
                                                                               typeof(EdgeControl),
                                                                               new UIPropertyMetadata());*/
        /// <summary>
        /// Gets or sets offset for the image position
        /// </summary>
        public Point Offset
        {
            get;// { return (Point)GetValue(OffsetProperty); }
            set;// { SetValue(OffsetProperty, value); }
        }

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

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
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

        #endregion


        private EdgeControl _edgeControl;
        protected EdgeControl EdgeControl { get { return _edgeControl ?? (_edgeControl = GetEdgeControl(VisualParent)); } }

        public EdgePointerImage()
        {
            RenderTransformOrigin = new Point(.5, .5);
            LayoutUpdated += EdgePointerImage_LayoutUpdated;
        }

        void EdgePointerImage_LayoutUpdated(object sender, EventArgs e)
        {
            if (LastKnownRectSize == Rect.Empty || double.IsNaN(LastKnownRectSize.Width) || LastKnownRectSize.Width == 0)
            {
                UpdateLayout();
                if (EdgeControl != null && !EdgeControl.IsSelfLooped)
                {
                    EdgeControl.UpdateEdge(false);
                }
            }
            else Arrange(LastKnownRectSize);
        }

        /// <summary>
        /// Update edge pointer position and angle
        /// </summary>
        public virtual Point Update(Point? position, Vector direction, double angle = 0d)
        {
            //var vecOffset = new Vector(direction.X * Offset.X, direction.Y * Offset.Y);
            if (DesiredSize.Width == 0 || DesiredSize.Height == 0) return new Point();
            position = position - new Vector(direction.X * DesiredSize.Width * .5, direction.Y * DesiredSize.Height * .5);// + vecOffset;

            if (position.HasValue && DesiredSize != Size.Empty)
            {
                LastKnownRectSize = new Rect(new Point(position.Value.X - DesiredSize.Width * .5, position.Value.Y - DesiredSize.Height * .5), DesiredSize);
                Arrange(LastKnownRectSize);
            }

            if(NeedRotation)
                RenderTransform = new RotateTransform(angle, 0, 0);
            return new Point(direction.X * DesiredSize.Width, direction.Y * DesiredSize.Height); ;
        }

        public void Dispose()
        {
            _edgeControl = null;
        }
    }
}
