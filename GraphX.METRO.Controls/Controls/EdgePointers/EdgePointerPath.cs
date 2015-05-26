using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using GraphX.Measure;
using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;

namespace GraphX.Controls
{
    public class EdgePointerPath: ContentControl, IEdgePointer
    {

        #region PATH part

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(Path), typeof(EdgePointerPath), new PropertyMetadata(null, PathChangedCallback));

        private static void PathChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var parent = dependencyObject as EdgePointerPath;
            if (parent == null)
                throw new Exception("EdgePointerPath -> ImageChangedCallback: Parent not found!");
            parent.Content = dependencyPropertyChangedEventArgs.NewValue;
            parent.ScalePath(parent.Scale);
        }

        /// <summary>
        /// Gets or sets geometry that specifies the path to be drawn
        /// </summary>
        public Path Path { get { return (Path)GetValue(PathProperty); } set { SetValue(PathProperty, value); } }

        #endregion

        #region Common 

        internal Rect LastKnownRectSize;

        public static readonly DependencyProperty NeedRotationProperty = DependencyProperty.Register("NeedRotation",
                                                                                       typeof(bool),
                                                                                       typeof(EdgePointerPath),
                                                                                       new PropertyMetadata(true));
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
        protected EdgeControl EdgeControl { get { return _edgeControl ?? (_edgeControl = GetEdgeControl(Parent)); } }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale",
                                                                               typeof(Point),
                                                                               typeof(EdgePointerPath),
                                                                               new PropertyMetadata(new Point(1,1), ScalePropertyChangedCallback));

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
            LayoutUpdated += EdgePointerPath_LayoutUpdated;
        }

        void EdgePointerPath_LayoutUpdated(object sender, object e)
        {
            if (LastKnownRectSize != Rect.Empty && !double.IsNaN(LastKnownRectSize.Width) && LastKnownRectSize.Width != 0) 
                Arrange(LastKnownRectSize);
        }


        /// <summary>
        /// Scales path by provided value
        /// </summary>
        /// <param name="scale">Point scale value</param>
        public void ScalePath(Point scale)
        {
            if(Path != null)
                Path.Data.Transform = new ScaleTransform() { ScaleX = scale.X, ScaleY = scale.Y };
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
            var vec = new Vector(direction.X * ActualWidth * .5, direction.Y * ActualHeight * .5);
            position = position.HasValue ? (Point?)new Point(position.Value.X - vec.X, position.Value.Y - vec.Y) : null;

            if (!double.IsNaN(position.Value.X))
            {
                LastKnownRectSize = new Rect(new Point(position.Value.X - DesiredSize.Width * .5, position.Value.Y - DesiredSize.Height * .5), DesiredSize);
                //Measure(LastKnownRectSize.Size());
                Arrange(LastKnownRectSize);
            }

            if(NeedRotation)
                RenderTransform = new RotateTransform() { Angle = angle, CenterX = 0, CenterY = 0 };
            //LastKnownRectSize = new Rect(0,0, 1, 1);
            return new Point(direction.X * ActualWidth, direction.Y * ActualHeight);
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
