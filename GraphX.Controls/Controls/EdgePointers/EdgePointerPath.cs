using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphX.WPF.Controls
{
    public class EdgePointerPath: Shape, IEdgePointer
    {

        #region PATH part

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Geometry), typeof(EdgePointerPath), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), null);

        /// <summary>
        /// Gets or sets geometry that specifies the path to be drawn
        /// </summary>
        public Geometry Data { get { return (Geometry)GetValue(DataProperty); } set { SetValue(DataProperty, value); } }

        protected override Geometry DefiningGeometry { get { return Data ?? Geometry.Empty; } }

        #endregion

        #region Common 

        internal Rect LastKnownRectSize;

        public static readonly DependencyProperty NeedRotationProperty = DependencyProperty.Register("NeedRotation",
                                                                                       typeof(bool),
                                                                                       typeof(EdgePointerPath),
                                                                                       new UIPropertyMetadata(true));
        /// <summary>
        /// Gets or sets if image has to be rotated according to edge directions
        /// </summary>
        public bool NeedRotation
        {
            get { return (bool)GetValue(NeedRotationProperty); }
            set { SetValue(NeedRotationProperty, value); }
        }

        #endregion

        private EdgeControl _edgeControl;
        protected EdgeControl EdgeControl { get { return _edgeControl ?? (_edgeControl = GetEdgeControl(VisualParent)); } }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale",
                                                                               typeof(Point),
                                                                               typeof(EdgePointerPath),
                                                                               new UIPropertyMetadata(new Point(1,1), ScalePropertyChangedCallback));

        private static void ScalePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var obj = (EdgePointerPath) dependencyObject;
            obj.ScalePath((Point)e.NewValue);
        }


        /// <summary>
        /// Gets or sets path scale multiplier
        /// </summary>
        public Point Scale
        {
            get { return (Point)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public EdgePointerPath()
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
        /// Scales path by provided value
        /// </summary>
        /// <param name="scale">Point scale value</param>
        public void ScalePath(Point scale)
        {
            var pathGeometry = Data.Clone();
            pathGeometry.Transform = new ScaleTransform(scale.X, scale.Y);
            Data = pathGeometry.GetFlattenedPathGeometry();
        }
        /// <summary>
        /// Scales path by provided value
        /// </summary>
        /// <param name="x">X scale value</param>
        /// <param name="y">Y scale value</param>
        public void ScalePath(double x, double y)
        {
            ScalePath(new Point(x,y));
        }
        /// <summary>
        /// Scales path by provided value
        /// </summary>
        /// <param name="value">Scale value</param>
        public void ScalePath(double value)
        {
            ScalePath(new Point(value, value));
        }

        /// <summary>
        /// Update edge pointer position and angle
        /// </summary>
        public virtual Point Update(Point? position, Vector direction, double angle = 0d)
        {
            if (DesiredSize.Width == 0 || DesiredSize.Height == 0) return new Point();
            position = position - new Vector(direction.X * DesiredSize.Width * .5, direction.Y * DesiredSize.Height * .5);

            if (position.HasValue && DesiredSize != Size.Empty)
            {
                LastKnownRectSize = new Rect(new Point(position.Value.X - DesiredSize.Width * .5, position.Value.Y - DesiredSize.Height * .5), DesiredSize);
                Arrange(LastKnownRectSize);
            }

            if(NeedRotation)
                RenderTransform = new RotateTransform(angle, 0, 0);
            return new Point(direction.X * DesiredSize.Width, direction.Y * DesiredSize.Height);
        }

        public void Dispose()
        {
            _edgeControl = null;
        }

        #region Common methods
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
    }
}
