using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using GraphX.Measure;
using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;

namespace GraphX.METRO.Controls
{
    public class EdgePointerImage: ContentControl, IEdgePointer
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

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image",
                                                                               typeof(Image),
                                                                               typeof(EdgePointerImage),
                                                                               new PropertyMetadata(null, ImageChangedCallback));

        private static void ImageChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var parent = dependencyObject as EdgePointerImage;
            if(parent == null)
                throw new Exception("EdgePointerImage -> ImageChangedCallback: Parent not found!");
            parent.Content = dependencyPropertyChangedEventArgs.NewValue;
        }

        /// <summary>
        /// Image for edge pointer
        /// </summary>
        public Image Image
        {
            get { return (Image)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }


        private EdgeControl _edgeControl;
        protected EdgeControl EdgeControl { get { return _edgeControl ?? (_edgeControl = GetEdgeControl(Parent)); } }

        public EdgePointerImage()
        {
            RenderTransformOrigin = new Point(.5, .5);
            LayoutUpdated +=EdgePointerImage_LayoutUpdated;
        }

        void EdgePointerImage_LayoutUpdated(object sender, object e)
        {
            if (LastKnownRectSize != Rect.Empty && !double.IsNaN(LastKnownRectSize.Width) && LastKnownRectSize.Width != 0)
                Arrange(LastKnownRectSize);
        }

        /// <summary>
        /// Update edge pointer position and angle
        /// </summary>
        public Point Update(Point? position, Vector direction, double angle = 0d)
        {
            //var vecOffset = new Vector(direction.X * Offset.X, direction.Y * Offset.Y);
            if (DesiredSize.Width == 0 || DesiredSize.Height == 0) return new Point();
            var vecMove = new Vector(direction.X * ActualWidth * .5, direction.Y * ActualHeight * .5);
            position = new Point(position.Value.X - vecMove.X, position.Value.Y - vecMove.Y);// + vecOffset;

            if (DesiredSize.Width != 0 && !double.IsNaN(position.Value.X))
            {
                LastKnownRectSize = new Rect(new Point(position.Value.X - DesiredSize.Width * .5, position.Value.Y - DesiredSize.Height * .5), DesiredSize);
                Measure(LastKnownRectSize.Size());
                Arrange(LastKnownRectSize);
            }

            if(NeedRotation)
                RenderTransform = new RotateTransform { Angle = angle, CenterX = 0, CenterY = 0 };
            return new Point(direction.X * ActualWidth, direction.Y * ActualHeight);
        }

        public void Dispose()
        {
            _edgeControl = null;
        }
    }
}
