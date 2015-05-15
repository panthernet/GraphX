using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;
using Point = Windows.Foundation.Point;

namespace GraphX.METRO.Controls
{
    [Bindable]
    public class EdgeLabelControl : ContentControl, IEdgeLabelControl
    {

        public static readonly DependencyProperty DisplayForSelfLoopedEdgesProperty = DependencyProperty.Register("DisplayForSelfLoopedEdges",
                                                                       typeof(bool),
                                                                       typeof(EdgeLabelControl),
                                                                       new PropertyMetadata(false));
        /// <summary>
        /// Gets or sets if label should be visible for self looped edge
        /// </summary>
        public bool DisplayForSelfLoopedEdges
        {
            get
            {
                return (bool)GetValue(DisplayForSelfLoopedEdgesProperty);
            }
            set
            {
                SetValue(DisplayForSelfLoopedEdgesProperty, value);
            }
        }

       public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle",
                                                                                       typeof(double),
                                                                                       typeof(EdgeLabelControl),
                                                                                       new PropertyMetadata(0.0, AngleChanged));

       private static void AngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
       {
           var ctrl = d as UIElement;
           if (ctrl == null)
           {
               Debug.WriteLine("VertexLabelControl -> dp isn't uielement!");
               return;
           }
           var tg = ctrl.RenderTransform as TransformGroup;
           if (tg == null) ctrl.RenderTransform = new RotateTransform { Angle = (double)e.NewValue, CenterX = .5, CenterY = .5 };
           else
           {
               var rt = tg.Children.FirstOrDefault(a => a is RotateTransform);
               if (rt == null)
                   tg.Children.Add(new RotateTransform { Angle = (double)e.NewValue, CenterX = .5, CenterY = .5 });
               else (rt as RotateTransform).Angle = (double)e.NewValue;
           }
       }

       private EdgeControl _edgeControl;
       protected EdgeControl EdgeControl { get { return _edgeControl ?? (_edgeControl = GetEdgeControl(Parent)); } }


        /// <summary>
        /// Gets or sets label drawing angle in degrees
        /// </summary>
        public double Angle
        {
            get
            {
                return (double)GetValue(AngleProperty);
            }
            set
            {
                SetValue(AngleProperty, value);
            }
        }

        public void Show()
        {
            if (EdgeControl.IsSelfLooped && !DisplayForSelfLoopedEdges) return;
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        public Rect GetSize()
        {
            return LastKnownRectSize.ToGraphX();
        }

        public void SetSize(Windows.Foundation.Rect size)
        {
            LastKnownRectSize = size;
        }

        public EdgeLabelControl()
        {
            DefaultStyleKey = typeof(EdgeLabelControl);
            if (DesignMode.DesignModeEnabled) return;

            LayoutUpdated += EdgeLabelControl_LayoutUpdated;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Loaded += EdgeLabelControl_Loaded;
        }

        void EdgeLabelControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (EdgeControl.IsSelfLooped && !DisplayForSelfLoopedEdges) Hide();
            else Show();
        }

        void EdgeLabelControl_LayoutUpdated(object sender, object e)
        {
            if (EdgeControl == null || !EdgeControl.ShowLabel) return;
            if (LastKnownRectSize == Windows.Foundation.Rect.Empty || double.IsNaN(LastKnownRectSize.Width) || LastKnownRectSize.Width == 0)
            {
                UpdateLayout();
                UpdatePosition();
            } 
            else Arrange(LastKnownRectSize);
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

        private static double GetLabelDistance(double edgeLength)
        {
            return edgeLength *.5;  // set the label halfway the length of the edge
        }

        public void UpdatePosition()
        {
            if (double.IsNaN(DesiredSize.Width) || DesiredSize.Width == 0) return;

           // if (!IsLoaded)
           //     return;
            if (EdgeControl == null)
                return;
            if (EdgeControl.Source == null || EdgeControl.Target == null)
            {
                Debug.WriteLine("EdgeLabelControl_LayoutUpdated() -> Got empty edgecontrol!");
                return;
            }

            //if hidden
            if (Visibility != Visibility.Visible) return;

            if (EdgeControl.IsSelfLooped)
            {
                var idesiredSize = DesiredSize;
                var pt = EdgeControl.Source.GetCenterPosition();
                pt = pt.Offset(-idesiredSize.Width / 2, (EdgeControl.Source.DesiredSize.Height * .5) + 2 + (idesiredSize.Height * .5));
                LastKnownRectSize = new Windows.Foundation.Rect(pt.X, pt.Y, idesiredSize.Width, idesiredSize.Height);
                Arrange(LastKnownRectSize);
                return;
            }

            var p1 = EdgeControl.SourceConnectionPoint.GetValueOrDefault();
            var p2 = EdgeControl.TargetConnectionPoint.GetValueOrDefault();

            double edgeLength = 0;
            var routingInfo = EdgeControl.Edge as IRoutingInfo;
            if (routingInfo != null) 
            {
                var routePoints =  routingInfo.RoutingPoints == null ? null : routingInfo.RoutingPoints.ToWindows();

                if (routePoints == null || routePoints.Length == 0)
                {
                    // the edge is a single segment (p1,p2)
                    edgeLength = GetLabelDistance(MathHelper.GetDistanceBetweenPoints(p1, p2));
                }
                else
                {
                    // the edge has one or more segments
                    // compute the total length of all the segments
                    edgeLength = 0;
                    var rplen = routePoints.Length;
                    for (var i = 0; i <= rplen; ++i)
                        if (i == 0)
                            edgeLength += MathHelper.GetDistanceBetweenPoints(p1, routePoints[0]);
                        else if (i == rplen)
                            edgeLength += MathHelper.GetDistanceBetweenPoints(routePoints[rplen - 1], p2);
                        else
                            edgeLength += MathHelper.GetDistanceBetweenPoints(routePoints[i - 1], routePoints[i]);
                    // find the line segment where the half distance is located
                    edgeLength = GetLabelDistance(edgeLength);
                    var newp1 = p1;
                    var newp2 = p2;
                    for (var i = 0; i <= rplen; ++i)
                    {
                        double lengthOfSegment;
                        if (i == 0)
                            lengthOfSegment = MathHelper.GetDistanceBetweenPoints(newp1 = p1, newp2 = routePoints[0]);
                        else if (i == rplen)
                            lengthOfSegment = MathHelper.GetDistanceBetweenPoints(newp1 = routePoints[rplen - 1], newp2 = p2);
                        else
                            lengthOfSegment = MathHelper.GetDistanceBetweenPoints(newp1 = routePoints[i - 1], newp2 = routePoints[i]);
                        if (lengthOfSegment >= edgeLength)
                            break;
                        edgeLength -= lengthOfSegment;
                    }
                    // redefine our edge points
                    p1 = newp1;
                    p2 = newp2;
                }
            }
            // The label control should be laid out on a rectangle, in the middle of the edge
            var angleBetweenPoints = MathHelper.GetAngleBetweenPoints(p1, p2);
            var desiredSize = DesiredSize;
            bool flipAxis = p1.X > p2.X; // Flip axis if source is "after" target

            // Calculate the center point of the edge
            var centerPoint = new Point(p1.X + edgeLength * Math.Cos(angleBetweenPoints), p1.Y - edgeLength * Math.Sin(angleBetweenPoints));
            if (EdgeControl.AlignLabelsToEdges)
            {
                // If we're aligning labels to the edges make sure add the label vertical offset
                var yEdgeOffset = EdgeControl.LabelVerticalOffset;
                if (flipAxis) // If we've flipped axis, move the offset to the other side of the edge
                    yEdgeOffset = -yEdgeOffset;

                // Adjust offset for rotation. Remember, the offset is perpendicular from the edge tangent.
                // Slap on 90 degrees to the angle between the points, to get the direction of the offset.
                centerPoint.Y -= yEdgeOffset * Math.Sin(angleBetweenPoints + Math.PI / 2);
                centerPoint.X += yEdgeOffset * Math.Cos(angleBetweenPoints + Math.PI / 2);

                // Angle is in degrees
                Angle = -angleBetweenPoints * 180 / Math.PI;
                if (flipAxis)
                    Angle += 180; // Reorient the label so that it's always "pointing north"
            }

            if (double.IsNaN(centerPoint.X)) centerPoint.X = 0;
            if (double.IsNaN(centerPoint.Y)) centerPoint.Y = 0;
            LastKnownRectSize = new Windows.Foundation.Rect(centerPoint.X - desiredSize.Width / 2, centerPoint.Y - desiredSize.Height / 2, desiredSize.Width, desiredSize.Height);
            Arrange(LastKnownRectSize);
        }

        internal Windows.Foundation.Rect LastKnownRectSize;

        public void Dispose()
        {
            _edgeControl = null;
        }
    }
}
