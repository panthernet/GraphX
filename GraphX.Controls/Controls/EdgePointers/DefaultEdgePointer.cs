#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DefaultEventArgs = System.EventArgs;
#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using GraphX.Measure;
using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;
using DefaultEventArgs = System.Object;
#endif

namespace GraphX.Controls
{
    /// <summary>
    /// Edge pointer control for edge endpoints customization
    /// Represents ContentControl that can host different content, e.g. Image or Path
    /// </summary>
    public class DefaultEdgePointer: ContentControl, IEdgePointer
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
                                                                                       new PropertyMetadata(true));
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
        protected EdgeControl EdgeControl { get { return _edgeControl ?? (_edgeControl = GetEdgeControl(GetParent())); } }

        public DefaultEdgePointer()
        {
            RenderTransformOrigin = new Point(.5, .5);
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            LayoutUpdated += EdgePointer_LayoutUpdated;
        }

        /// <summary>
        /// Update edge pointer position and angle
        /// </summary>
        public virtual Point Update(Point? position, Vector direction, double angle = 0d)
        {
            //var vecOffset = new Vector(direction.X * Offset.X, direction.Y * Offset.Y);
            if (DesiredSize.Width == 0 || DesiredSize.Height == 0 || !position.HasValue) return new Point();
            var vecMove = new Vector(direction.X * DesiredSize.Width * .5, direction.Y * DesiredSize.Height * .5);
            position = new Point(position.Value.X - vecMove.X, position.Value.Y - vecMove.Y);// + vecOffset;
            if (!double.IsNaN(DesiredSize.Width) && DesiredSize.Width != 0  && !double.IsNaN(position.Value.X))
            {
                LastKnownRectSize = new Rect(new Point(position.Value.X - DesiredSize.Width * .5, position.Value.Y - DesiredSize.Height * .5), DesiredSize);
                Arrange(LastKnownRectSize);
            }

            if(NeedRotation)
                RenderTransform = new RotateTransform { Angle = angle, CenterX = 0, CenterY = 0 };
            return new Point(direction.X * ActualWidth, direction.Y * ActualHeight);
        }

        public void SetManualPosition(Point position)
        {
            LastKnownRectSize = new Rect(new Point(position.X - DesiredSize.Width * .5, position.Y - DesiredSize.Height * .5), DesiredSize);
            Arrange(LastKnownRectSize);
            
        }

        public void Dispose()
        {
            _edgeControl = null;
        }

        void EdgePointer_LayoutUpdated(object sender, DefaultEventArgs e)
        {
            if (LastKnownRectSize != Rect.Empty && !double.IsNaN(LastKnownRectSize.Width) && LastKnownRectSize.Width != 0
                && EdgeControl != null)
                Arrange(LastKnownRectSize);
        }

        DependencyObject GetParent()
        {
#if WPF
            return VisualParent;
#elif METRO
            return Parent;
#endif
        }
    }
}
