using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphX.Controls.Models.Interfaces;

namespace GraphX
{
    public class EdgeLabelControl : ContentControl, IEdgeLabelControl
    {
        internal Rect LastKnownRectSize;

        #region Common part

        public static readonly DependencyProperty DisplayForSelfLoopedEdgesProperty = DependencyProperty.Register("DisplayForSelfLoopedEdges",
                                                                               typeof(bool),
                                                                               typeof(EdgeLabelControl),
                                                                               new UIPropertyMetadata(false));
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
                                                                                       new UIPropertyMetadata(0.0));
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

        private EdgeControl _edgeControl;
        protected EdgeControl EdgeControl { get { return _edgeControl ?? (_edgeControl = GetEdgeControl(VisualParent)); } }

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

        public void Show()
        {
            if (EdgeControl.IsSelfLooped && !DisplayForSelfLoopedEdges) return;
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
        #endregion

        public EdgeLabelControl()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            LayoutUpdated += EdgeLabelControl_LayoutUpdated;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            this.Initialized += EdgeLabelControl_Initialized;
        }

        void EdgeLabelControl_Initialized(object sender, EventArgs e)
        {
            if (EdgeControl.IsSelfLooped && !DisplayForSelfLoopedEdges) Hide();
            else Show();
        }

        void EdgeLabelControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (EdgeControl == null || !EdgeControl.ShowLabel) return;
            if (LastKnownRectSize == Rect.Empty || double.IsNaN(LastKnownRectSize.Width) || LastKnownRectSize.Width == 0)
            {
                UpdateLayout();
                UpdatePosition();
            } 
            else Arrange(LastKnownRectSize);
        }

        

        private static double GetLabelDistance(double edgeLength)
        {
            return edgeLength / 2;  // set the label halfway the length of the edge
        }


        /// <summary>
        /// Automaticaly update edge label position
        /// </summary>
        public virtual void UpdatePosition()
        {
            if (double.IsNaN(DesiredSize.Width) || DesiredSize.Width == 0) return;

            if (EdgeControl == null)
                return;
            if (EdgeControl.Source == null || EdgeControl.Target == null)
            {
                Debug.WriteLine("EdgeLabelControl_LayoutUpdated() -> Got empty edgecontrol!");
                return;
            }
            //if hidden
            if (Visibility != Visibility.Visible) return;

            if(EdgeControl.IsSelfLooped)
            {
                var idesiredSize = DesiredSize;
                var pt = EdgeControl.Source.GetCenterPosition();
                pt.Offset(-idesiredSize.Width / 2, (EdgeControl.Source.DesiredSize.Height * .5) + 2 + (idesiredSize.Height * .5));
                LastKnownRectSize = new Rect(pt.X, pt.Y, idesiredSize.Width, idesiredSize.Height);
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
            // align the point so that it  passes through the center of the label content
            var p = p1;
            var desiredSize = DesiredSize;
            p.Offset(-desiredSize.Width / 2, -desiredSize.Height / 2);

            // move it "edgLength" on the segment
            double tmpAngle;
            var angleBetweenPoints = tmpAngle = MathHelper.GetAngleBetweenPoints(p1, p2);
            //set angle in degrees
            if (EdgeControl.AlignLabelsToEdges)
            {
                if (p1.X > p2.X)
                    tmpAngle = MathHelper.GetAngleBetweenPoints(p2, p1);
                Angle = -tmpAngle * 180 / Math.PI;
            }
  
            p.Offset(edgeLength * Math.Cos(angleBetweenPoints), -edgeLength * Math.Sin(angleBetweenPoints));
            if (EdgeControl.AlignLabelsToEdges)
                p = MathHelper.RotatePoint(new Point(p.X, p.Y - EdgeControl.LabelVerticalOffset), p, Angle);
            //optimized offset here
            /*float x = 12.5f, y = 12.5f;
            double sin = Math.Sin(angleBetweenPoints);
            double cos = Math.Cos(angleBetweenPoints);
            double sign = sin * cos / Math.Abs(sin * cos);
            p.Offset(x * sin * sign + edgeLength * cos, y * cos * sign - edgeLength * sin);*/
            LastKnownRectSize = new Rect(p.X,p.Y, desiredSize.Width, desiredSize.Height);
            Arrange(LastKnownRectSize);
        }

        /// <summary>
        /// Get label rectangular size
        /// </summary>
        public Rect GetSize()
        {
            return LastKnownRectSize;
        }
        /// <summary>
        /// Set label rectangular size
        /// </summary>
        public void SetSize(Rect size)
        {
            LastKnownRectSize = size;
            Arrange(LastKnownRectSize);
        }

        public void Dispose()
        {
            _edgeControl = null;
        }
    }
}
