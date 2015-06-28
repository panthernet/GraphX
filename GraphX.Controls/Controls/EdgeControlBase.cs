using System;
using System.Linq;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
#if WPF 
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using SysRect = System.Windows.Rect;
#elif METRO
using GraphX.Measure;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Point = Windows.Foundation.Point;
using SysRect = Windows.Foundation.Rect;
using Size = Windows.Foundation.Size;
#endif

namespace GraphX.Controls
{
    [TemplatePart(Name = "PART_edgePath", Type = typeof(Path))]
    [TemplatePart(Name = "PART_SelfLoopedEdge", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_edgeArrowPath", Type = typeof(Path))]//obsolete, present for exception
    [TemplatePart(Name = "PART_edgeLabel", Type = typeof(IEdgeLabelControl))]
    [TemplatePart(Name = "PART_EdgePointerForSource", Type = typeof(IEdgePointer))]
    [TemplatePart(Name = "PART_EdgePointerForTarget", Type = typeof(IEdgePointer))]
    public abstract class EdgeControlBase : Control, IGraphControl, IDisposable
    {
#if METRO
        void IPositionChangeNotify.OnPositionChanged()
        {
            //skip any actions on own position change
        }
#endif

        #region Properties & Fields

        public abstract bool IsSelfLooped { get; protected set; }
        public abstract void Dispose();
        public abstract void Clean();
        protected DoubleCollection StrokeDashArray { get; set; }

        /// <summary>
        /// Gets if this edge is parallel (has another edge with the same source and target vertices)
        /// </summary>
        public bool IsParallel { get; internal set; }

        /// <summary>
        /// Element presenting self looped edge
        /// </summary>
        protected FrameworkElement SelfLoopIndicator;

        /// <summary>
        /// Used to store last known SLE rect size for proper updates on layout passes
        /// </summary>
        private SysRect _selfLoopedEdgeLastKnownRect;

        protected virtual void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
        protected virtual void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        /// <summary>
        /// Gets or sets parent GraphArea visual
        /// </summary>
        public GraphAreaBase RootArea
        {
            get { return (GraphAreaBase)GetValue(RootCanvasProperty); }
            set { SetValue(RootCanvasProperty, value); }
        }

        public static readonly DependencyProperty RootCanvasProperty =
            DependencyProperty.Register("RootArea", typeof(GraphAreaBase), typeof(EdgeControlBase), new PropertyMetadata(null));


        public static readonly DependencyProperty SelfLoopIndicatorRadiusProperty = DependencyProperty.Register("SelfLoopIndicatorRadius",
                                                                                       typeof(double),
                                                                                       typeof(EdgeControlBase),
                                                                                       new PropertyMetadata(5d));
        /// <summary>
        /// Radius of the default self-loop indicator, which is drawn as a circle (when custom template isn't provided). Default is 20.
        /// </summary>
        public double SelfLoopIndicatorRadius {
            get { return (double)GetValue(SelfLoopIndicatorRadiusProperty); }
            set { SetValue(SelfLoopIndicatorRadiusProperty, value); }
        }

        public static readonly DependencyProperty SelfLoopIndicatorOffsetProperty = DependencyProperty.Register("SelfLoopIndicatorOffset",
                                                                               typeof(Point),
                                                                               typeof(EdgeControlBase),
                                                                               new PropertyMetadata(new Point()));
        /// <summary>
        /// Offset from the left-top corner of the vertex. Useful for custom vertex shapes. Default is 10,10.
        /// </summary>
        public Point SelfLoopIndicatorOffset
        {
            get { return (Point)GetValue(SelfLoopIndicatorOffsetProperty); }
            set { SetValue(SelfLoopIndicatorOffsetProperty, value); }
        }

        public static readonly DependencyProperty ShowSelfLoopIndicatorProperty = DependencyProperty.Register("ShowSelfLoopIndicator",
                                                                       typeof(bool),
                                                                       typeof(EdgeControlBase),
                                                                       new PropertyMetadata(true));
        /// <summary>
        /// Show self looped edge indicator on the vertex top-left corner. Default value is true.
        /// </summary>
        public bool ShowSelfLoopIndicator
        {
            get { return (bool)GetValue(ShowSelfLoopIndicatorProperty); }
            set { SetValue(ShowSelfLoopIndicatorProperty, value); }
        }


        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source",
                                                                                               typeof(VertexControl),
                                                                                               typeof(EdgeControlBase),
                                                                                               new PropertyMetadata(null, OnSourceChangedInternal));

        private static void OnSourceChangedInternal(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as EdgeControlBase;
            if(ctrl != null) ctrl.OnSourceChanged(d, e);
        }

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target",
                                                                                               typeof(VertexControl),
                                                                                               typeof(EdgeControlBase),
                                                                                               new PropertyMetadata(null, OnTargetChangedInternal));

        private static void OnTargetChangedInternal(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as EdgeControlBase;
            if (ctrl != null) ctrl.OnTargetChanged(d, e);
        }


        public static readonly DependencyProperty EdgeProperty = DependencyProperty.Register("Edge", typeof(object),
                                                                                             typeof(EdgeControlBase),
                                                                                             new PropertyMetadata(null));

 

        private double _labelAngle;
        /// <summary>
        /// Gets or sets vertex label angle
        /// </summary>
        public double LabelAngle
        {
            get
            {
                return _edgeLabelControl != null ? _edgeLabelControl.Angle : _labelAngle;
            }
            set
            {
                _labelAngle = value;
                if (_edgeLabelControl != null) _edgeLabelControl.Angle = _labelAngle;
            }
        }

        #region DashStyle

        public static readonly DependencyProperty DashStyleProperty = DependencyProperty.Register("DashStyle",
                                                                                       typeof(EdgeDashStyle),
                                                                                       typeof(EdgeControlBase),
                                                                                       new PropertyMetadata(EdgeDashStyle.Solid, dashstyle_changed));

        private static void dashstyle_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ec = d as EdgeControlBase;
            if (ec == null) return;
            switch ((EdgeDashStyle)e.NewValue)
            {
                case EdgeDashStyle.Solid:
                    ec.StrokeDashArray = null;
                    break;

                case EdgeDashStyle.Dash:
                    ec.StrokeDashArray = new DoubleCollection { 4.0, 2.0 };
                    break;
                case EdgeDashStyle.Dot:
                    ec.StrokeDashArray = new DoubleCollection { 1.0, 2.0 };
                    break;

                case EdgeDashStyle.DashDot:
                    ec.StrokeDashArray = new DoubleCollection { 4.0, 2.0, 1.0, 2.0 };
                    break;

                case EdgeDashStyle.DashDotDot:
                    ec.StrokeDashArray = new DoubleCollection { 4.0, 2.0, 1.0, 2.0, 1.0, 2.0 };
                    break;

                default:
                    ec.StrokeDashArray = null;
                    break;
            }
            ec.UpdateEdge(false);
        }


        /// <summary>
        /// Gets or sets edge dash style
        /// </summary>
        public EdgeDashStyle DashStyle
        {
            get { return (EdgeDashStyle)GetValue(DashStyleProperty); }
            set { SetValue(DashStyleProperty, value); }
        }
        #endregion

        private bool _canbeparallel = true;
        /// <summary>
        /// Gets or sets if this edge can be paralellized if GraphArea.EnableParallelEdges is true.
        /// If not it will be drawn by default.
        /// </summary>
        public bool CanBeParallel { get { return _canbeparallel; } set { _canbeparallel = value; } }

        protected EdgeControlBase()
        {
            _updateLabelPosition = true;
        }

        private bool _updateLabelPosition;
        /// <summary>
        /// Gets or sets if label position should be updated on edge update
        /// </summary>
        public bool UpdateLabelPosition { get { return _updateLabelPosition; } set { _updateLabelPosition = true; } }

#if WPF
        protected PropertyChangeNotifier _sourceListener;
        protected PropertyChangeNotifier _targetListener;
#endif
        /// <summary>
        /// Gets or set if hidden edges should be updated when connected vertices positions are changed. Default value is True.
        /// </summary>
        public bool IsHiddenEdgesUpdated { get; set; }

        public static readonly DependencyProperty ShowArrowsProperty = DependencyProperty.Register("ShowArrows", typeof(bool), typeof(EdgeControl), new PropertyMetadata(true, showarrows_changed));

        private static void showarrows_changed(object sender, DependencyPropertyChangedEventArgs args)
        {
            var ctrl = sender as EdgeControl;
            if (ctrl == null)
                return;

            if (ctrl._edgePointerForSource != null && !ctrl.IsSelfLooped)
                if (ctrl.ShowArrows) ctrl._edgePointerForSource.Show(); else ctrl._edgePointerForSource.Hide();
            if (ctrl._edgePointerForTarget != null && !ctrl.IsSelfLooped)
                if (ctrl.ShowArrows) ctrl._edgePointerForTarget.Show(); else ctrl._edgePointerForTarget.Hide();
            ctrl.UpdateEdge(false);
        }

        /// <summary>
        /// Show arrows on the edge ends. Default value is true.
        /// </summary>
        public bool ShowArrows { get { return (bool)GetValue(ShowArrowsProperty); } set { SetValue(ShowArrowsProperty, value); } }

        public static readonly DependencyProperty ShowLabelProperty = DependencyProperty.Register("ShowLabel",
                                                                               typeof (bool),
                                                                               typeof (EdgeControl),
                                                                               new PropertyMetadata(false, showlabel_changed));

        private static void showlabel_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ec = (d as EdgeControl);
            if (ec == null) return;

            ec.UpdateEdge(false);
        }
        /// <summary>
        /// Show edge label.Default value is False.
        /// </summary>
        public bool ShowLabel { get { return (bool)GetValue(ShowLabelProperty); } set { SetValue(ShowLabelProperty, value); } }

        /// <summary>
        /// Gets or sets if lables should be aligned to edges and be displayed under the same angle
        /// </summary>
        public bool AlignLabelsToEdges
        {
            get { return _alignLabelsToEdges; }
            set
            {
                _alignLabelsToEdges = value;
                if (_edgeLabelControl != null)
                {
                    if (value == false) _edgeLabelControl.Angle = 0;
                    _edgeLabelControl.UpdatePosition();
                }
            }
        }
        protected bool _alignLabelsToEdges;

        public static readonly DependencyProperty LabelVerticalOffsetProperty = DependencyProperty.Register("LabelVerticalOffset",
                                                                               typeof(double),
                                                                               typeof(EdgeControl),
                                                                               new PropertyMetadata(0d));

        /// <summary>
        /// Offset for labels Y axis to display it above/below the edge
        /// </summary>
        public double LabelVerticalOffset { get { return (double)GetValue(LabelVerticalOffsetProperty); } set { SetValue(LabelVerticalOffsetProperty, value); } }

        /// <summary>
        ///  Gets or Sets that user controls the path geometry object or it is generated automatically
        /// </summary>
        public bool ManualDrawing { get; set; }

        /// <summary>
        /// Geometry object that represents visual edge path. Applied in OnApplyTemplate and OnRender.
        /// </summary>
        protected Geometry _linegeometry;

        /// <summary>
        /// Templated Path object to operate with routed path
        /// </summary>
        protected Path _linePathObject;

        /// <summary>
        /// Templated label control to display labels
        /// </summary>
        protected IEdgeLabelControl _edgeLabelControl;

        protected IEdgePointer _edgePointerForSource;
        protected IEdgePointer _edgePointerForTarget;

        public EdgeEventOptions EventOptions { get; protected set; }

        /// <summary>
        /// Source visual vertex object
        /// </summary>
        public VertexControl Source
        {
            get { return (VertexControl)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        /// <summary>
        /// Target visual vertex object
        /// </summary>
        public VertexControl Target
        {
            get { return (VertexControl)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }


        /// <summary>
        /// Data edge object
        /// </summary>
        public object Edge
        {
            get { return GetValue(EdgeProperty); }
            set { SetValue(EdgeProperty, value); }
        }

        #endregion

        #region Position methods

        /// <summary>
        /// Set attached coordinates X and Y
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="alsoFinal"></param>
        public void SetPosition(Point pt, bool alsoFinal = true)
        {
            GraphAreaBase.SetX(this, pt.X, alsoFinal);
            GraphAreaBase.SetY(this, pt.Y, alsoFinal);
        }

        public void SetPosition(double x, double y, bool alsoFinal = true)
        {
            GraphAreaBase.SetX(this, x, alsoFinal);
            GraphAreaBase.SetY(this, y, alsoFinal);
        }

        /// <summary>
        /// Get control position on the GraphArea panel in attached coords X and Y
        /// </summary>
        /// <param name="final"></param>
        /// <param name="round"></param>
        public Point GetPosition(bool final = false, bool round = false)
        {
            return new Point(final ? GraphAreaBase.GetFinalX(this) : GraphAreaBase.GetX(this), final ? GraphAreaBase.GetFinalY(this) : GraphAreaBase.GetY(this));
        }

        /// <summary>
        /// Get control position on the GraphArea panel in attached coords X and Y
        /// </summary>
        /// <param name="final"></param>
        /// <param name="round"></param>
        internal Measure.Point GetPositionGraphX(bool final = false, bool round = false)
        {
            return new Measure.Point(final ? GraphAreaBase.GetFinalX(this) : GraphAreaBase.GetX(this), final ? GraphAreaBase.GetFinalY(this) : GraphAreaBase.GetY(this));
        }
        #endregion

        #region Manual path controls
        /// <summary>
        /// Gets current edge path geometry object
        /// </summary>
        public PathGeometry GetEdgePathManually()
        {
            if (!ManualDrawing) return null;
            return _linegeometry as PathGeometry;
        }

        /// <summary>
        /// Sets current edge path geometry object
        /// </summary>
        public void SetEdgePathManually(PathGeometry geo)
        {
            if (!ManualDrawing) return;
            _linegeometry = geo;
            UpdateEdge();
        }
        #endregion

        internal virtual void InvalidateChildren()
        {
            if (_edgeLabelControl != null)
                _edgeLabelControl.UpdateLayout();
            if (_linePathObject != null)
            {
                var pos = Source.GetPosition();
                Source.SetPosition(pos.X, pos.Y);
            }
        }

        /// <summary>
        /// Gets if Template has been loaded and edge can operate at 100%
        /// </summary>
        public bool IsTemplateLoaded
        {
            get { return _linePathObject != null; }
        }
#if WPF
        public override void OnApplyTemplate()
#elif METRO
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();

            if (Template == null) return;

            _linePathObject = GetTemplatePart("PART_edgePath") as Path;
            if (_linePathObject == null) throw new GX_ObjectNotFoundException("EdgeControl Template -> Edge template must contain 'PART_edgePath' Path object to draw route points!");
            _linePathObject.Data = _linegeometry;
            if (this.FindDescendantByName("PART_edgeArrowPath") != null)
                throw new GX_ObsoleteException("PART_edgeArrowPath is obsolete! Please use new DefaultEdgePointer object in your EdgeControl template!");

            _edgeLabelControl = GetTemplatePart("PART_edgeLabel") as IEdgeLabelControl;

            _edgePointerForSource = GetTemplatePart("PART_EdgePointerForSource") as IEdgePointer;
            _edgePointerForTarget = GetTemplatePart("PART_EdgePointerForTarget") as IEdgePointer;

            SelfLoopIndicator = GetTemplatePart("PART_SelfLoopedEdge") as FrameworkElement;
            if(SelfLoopIndicator != null)
                SelfLoopIndicator.LayoutUpdated += (sender, args) =>
                {
                    if (SelfLoopIndicator != null) SelfLoopIndicator.Arrange(_selfLoopedEdgeLastKnownRect);
                };

            MeasureChild(_edgePointerForSource as UIElement);
            MeasureChild(_edgePointerForTarget as UIElement);
            MeasureChild(SelfLoopIndicator);
            //TODO measure label?

            UpdateSelfLoopedEdgeData();

            UpdateEdge();
        }

        /// <summary>
        /// Measure child objects such as template parts which are not updated automaticaly on first pass.
        /// </summary>
        /// <param name="child">Child UIElement</param>
        protected void MeasureChild(UIElement child)
        {
            if (child == null) return;
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        #region public PrepareEdgePath()

        /// <summary>
        /// Complete edge update pass. Don't needed to be run manualy until some edge related modifications are done requiring full edge update.
        /// </summary>
        /// <param name="updateLabel">Update label data</param>
        public virtual void UpdateEdge(bool updateLabel = true)
        {
            if (Visibility == Visibility.Visible || IsHiddenEdgesUpdated)
            {
                UpdateEdgeRendering(updateLabel);

                if (_edgeLabelControl != null)
                    if (ShowLabel) _edgeLabelControl.Show(); else _edgeLabelControl.Hide();
            }
        }

        /// <summary>
        /// Internal. Update only edge points andge edge line visual
        /// </summary>
        /// <param name="updateLabel"></param>
        internal virtual void UpdateEdgeRendering(bool updateLabel = true)
        {
            if (!IsTemplateLoaded)
                ApplyTemplate();
            PrepareEdgePath(true, null, updateLabel);
            if (_linePathObject == null) return;
            _linePathObject.Data = _linegeometry;
            _linePathObject.StrokeDashArray = StrokeDashArray;
        }


        internal int ParallelEdgeOffset;
        //internal int TargetOffset;

        /// <summary>
        /// Gets the offset point for edge parallelization
        /// </summary>
        /// <param name="source">Source vertex</param>
        /// <param name="target">Target vertex</param>
        /// <param name="sideDistance">Distance between edges</param>
        internal virtual Point GetParallelOffset(VertexControl source, VertexControl target, int sideDistance)
        {
            var sourcepos = source.GetPosition();
            var targetpos = target.GetPosition();

            var mainVector = new Vector(targetpos.X - sourcepos.X, targetpos.Y - sourcepos.Y);
            //get new point coordinate
            var joint = new Point(
                 sourcepos.X + source.DesiredSize.Width * .5 + sideDistance * (mainVector.Y / mainVector.Length),
                 sourcepos.Y + source.DesiredSize.Height * .5 - sideDistance * (mainVector.X / mainVector.Length));
            return joint;
        }
        /// <summary>
        /// Internal value to store last calculated Source vertex connection point
        /// </summary>
        internal Point? SourceConnectionPoint;
        /// <summary>
        /// Internal value to store last calculated Target vertex connection point
        /// </summary>
        internal Point? TargetConnectionPoint;

        /// <summary>
        ///Gets is looped edge indicator template available. Used to pass some heavy cycle checks.
        /// </summary>
        protected bool HasSelfLoopedEdgeTemplate { get { return SelfLoopIndicator != null; } }

        /// <summary>
        /// Update SLE data such as template, edge pointers visibility
        /// </summary>
        protected virtual void UpdateSelfLoopedEdgeData()
        {
            //generate object if template is present
            if (IsSelfLooped)
            {
                //hide edge pointers
                if (_edgePointerForSource != null) _edgePointerForSource.Hide();
                if (_edgePointerForTarget != null) _edgePointerForTarget.Hide();

                //return if we don't need to show edge loops
                if (!ShowSelfLoopIndicator) return;

                //pregenerate built-in indicator geometry if template PART is absent
                if (!HasSelfLoopedEdgeTemplate)
                    _linegeometry = new EllipseGeometry();
                else SelfLoopIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                if (_edgePointerForSource != null && ShowArrows) _edgePointerForSource.Show();
                if (_edgePointerForTarget != null && ShowArrows) _edgePointerForTarget.Show();

                if (HasSelfLoopedEdgeTemplate)
                    SelfLoopIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Process self looped edge positioning
        /// </summary>
        /// <param name="sourcePos">Left-top vertex position</param>
        protected virtual void PrepareSelfLoopedEdge(Point sourcePos)
        {
            if (!ShowSelfLoopIndicator)
                return;

            var hasNoTemplate = !HasSelfLoopedEdgeTemplate;
            var pt =
                new Point(
                    sourcePos.X + SelfLoopIndicatorOffset.X - (hasNoTemplate ? SelfLoopIndicatorRadius : SelfLoopIndicator.DesiredSize.Width),
                    sourcePos.Y + SelfLoopIndicatorOffset.X - (hasNoTemplate ? SelfLoopIndicatorRadius : SelfLoopIndicator.DesiredSize.Height));

            //if we has no self looped edge template defined we'll use default built-in indicator
            if (hasNoTemplate)
            {
                var geometry = _linegeometry as EllipseGeometry;
                geometry.Center = pt;
                geometry.RadiusX = SelfLoopIndicatorRadius;
                geometry.RadiusY = SelfLoopIndicatorRadius;
            }
            else _selfLoopedEdgeLastKnownRect = new SysRect(pt, SelfLoopIndicator.DesiredSize);
        }

        /// <summary>
        /// Create and apply edge path using calculated ER parameters stored in edge
        /// </summary>
        /// <param name="useCurrentCoords">Use current vertices coordinates or final coorfinates (for.ex if move animation is active final coords will be its destination)</param>
        /// <param name="externalRoutingPoints">Provided custom routing points will be used instead of stored ones.</param>
        /// <param name="updateLabel">Should edge label be updated in this pass</param>
        public virtual void PrepareEdgePath(bool useCurrentCoords = false, Measure.Point[] externalRoutingPoints = null, bool updateLabel = true)
        {
            //do not calculate invisible edges
            if ((Visibility != Visibility.Visible && !IsHiddenEdgesUpdated) && Source == null || Target == null || ManualDrawing || !IsTemplateLoaded) return;

            #region Get the inputs
            //get the size of the source
            var sourceSize = new Size
            {
                Width = Source.ActualWidth,
                Height = Source.ActualHeight
            };
            if (CustomHelper.IsInDesignMode(this)) sourceSize = new Size(80, 20);

            //get the position center of the source
            var sourcePos = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(Source) : GraphAreaBase.GetFinalX(Source)) + sourceSize.Width * .5,
                Y = (useCurrentCoords ? GraphAreaBase.GetY(Source) : GraphAreaBase.GetFinalY(Source)) + sourceSize.Height * .5
            };

            //get the size of the target
            var targetSize = new Size
            {
                Width = Target.ActualWidth,
                Height = Target.ActualHeight
            };
            if (CustomHelper.IsInDesignMode(this))
                targetSize = new Size(80, 20);

            //get the position center of the target
            var targetPos = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(Target) : GraphAreaBase.GetFinalX(Target)) + targetSize.Width * .5,
                Y = (useCurrentCoords ? GraphAreaBase.GetY(Target) : GraphAreaBase.GetFinalY(Target)) + targetSize.Height * .5
            };

            var routedEdge = Edge as IRoutingInfo;
            if (routedEdge == null)
                throw new GX_InvalidDataException("Edge must implement IRoutingInfo interface");

            //get the route informations
            var routeInformation = externalRoutingPoints ?? routedEdge.RoutingPoints;

            // Get the TopLeft position of the Source Vertex.
            var sourcePos1 = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(Source) : GraphAreaBase.GetFinalX(Source)),
                Y = (useCurrentCoords ? GraphAreaBase.GetY(Source) : GraphAreaBase.GetFinalY(Source))
            };
            // Get the TopLeft position of the Target Vertex.
            var targetPos1 = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(Target) : GraphAreaBase.GetFinalX(Target)),
                Y = (useCurrentCoords ? GraphAreaBase.GetY(Target) : GraphAreaBase.GetFinalY(Target))
            };

            var hasEpSource = _edgePointerForSource != null;
            var hasEpTarget = _edgePointerForTarget != null;
            #endregion

            //if self looped edge
            if (IsSelfLooped)
            {
                PrepareSelfLoopedEdge(sourcePos1);
                return;
            }

            //check if we have some edge route data
            var hasRouteInfo = routeInformation != null && routeInformation.Length > 1;

            //calculate source and target edge attach points
            if (RootArea != null && !hasRouteInfo && RootArea.EnableParallelEdges && ParallelEdgeOffset != 0)
            {
                sourcePos = GetParallelOffset(Source, Target, ParallelEdgeOffset);
                targetPos = GetParallelOffset(Target, Source, -ParallelEdgeOffset);
            }

            /* Rectangular shapes implementation by bleibold */

            var gEdge = Edge as IGraphXCommonEdge;
            Point p1;
            Point p2;

            //calculate edge source (p1) and target (p2) endpoints based on different settings
            if (gEdge != null && gEdge.SourceConnectionPointId.HasValue)
            {
                var sourceCp = Source.GetConnectionPointById(gEdge.SourceConnectionPointId.Value, true);
                if (sourceCp == null)
                    throw new GX_ObjectNotFoundException(string.Format("Can't find source vertex VCP by edge source connection point Id({1}) : {0}", Source, gEdge.SourceConnectionPointId));
                if (sourceCp.Shape == VertexShape.None) p1 = sourceCp.RectangularSize.Center();
                else
                {
                    var targetCpPos = gEdge.TargetConnectionPointId.HasValue ? Target.GetConnectionPointById(gEdge.TargetConnectionPointId.Value, true).RectangularSize.Center() : (hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos));
                    p1 = GeometryHelper.GetEdgeEndpoint(sourceCp.RectangularSize.Center(), sourceCp.RectangularSize, targetCpPos, sourceCp.Shape);
                }
            }
            else
                p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new SysRect(sourcePos1, sourceSize), (hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos)), Source.VertexShape);

            if (gEdge != null && gEdge.TargetConnectionPointId.HasValue)
            {
                var targetCp = Target.GetConnectionPointById(gEdge.TargetConnectionPointId.Value, true);
                if (targetCp == null)
                    throw new GX_ObjectNotFoundException(string.Format("Can't find target vertex VCP by edge target connection point Id({1}) : {0}", Target, gEdge.TargetConnectionPointId));
                if (targetCp.Shape == VertexShape.None) p2 = targetCp.RectangularSize.Center();
                else
                {
                    var sourceCpPos = gEdge.SourceConnectionPointId.HasValue ? Source.GetConnectionPointById(gEdge.SourceConnectionPointId.Value, true).RectangularSize.Center() : hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos);
                    p2 = GeometryHelper.GetEdgeEndpoint(targetCp.RectangularSize.Center(), targetCp.RectangularSize, sourceCpPos, targetCp.Shape);
                }
            }
            else
                p2 = GeometryHelper.GetEdgeEndpoint(targetPos, new SysRect(targetPos1, targetSize), hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos), Target.VertexShape);

            SourceConnectionPoint = p1;
            TargetConnectionPoint = p2;

            _linegeometry = new PathGeometry(); PathFigure lineFigure;

            //if we have route and route consist of 2 or more points
            if (RootArea != null && hasRouteInfo)
            {
                //replace start and end points with accurate ones
                var routePoints = routeInformation.ToWindows().ToList();
                routePoints.Remove(routePoints.First());
                routePoints.Remove(routePoints.Last());
                routePoints.Insert(0, p1);
                routePoints.Add(p2);

                if (RootArea.EdgeCurvingEnabled)
                {
                    var oPolyLineSegment = GeometryHelper.GetCurveThroughPoints(routePoints.ToArray(), 0.5, RootArea.EdgeCurvingTolerance);

                    if (hasEpTarget)
                    {
                        UpdateTargetEpData(oPolyLineSegment.Points[oPolyLineSegment.Points.Count - 1], oPolyLineSegment.Points[oPolyLineSegment.Points.Count - 2]);
                        oPolyLineSegment.Points.RemoveAt(oPolyLineSegment.Points.Count - 1);
                    }
                    if (hasEpSource) UpdateSourceEpData(oPolyLineSegment.Points.First(), oPolyLineSegment.Points[1]);

                    lineFigure = GeometryHelper.GetPathFigureFromPathSegments(routePoints[0], true, true, oPolyLineSegment);
#if WPF
                    //freeze and create resulting geometry
                    GeometryHelper.TryFreeze(oPolyLineSegment);
#endif
                }
                else
                {
                    if (hasEpSource) UpdateSourceEpData(routePoints.First(), routePoints[1]);
                    if (hasEpTarget)
                        routePoints[routePoints.Count - 1] = routePoints[routePoints.Count - 1].Subtract(UpdateTargetEpData(p2, routePoints[routePoints.Count - 2]));

                    var pcol = new PointCollection();
                    foreach (var item in routePoints)
                        pcol.Add(item);

                    lineFigure = new PathFigure { StartPoint = p1, Segments = new PathSegmentCollection { new PolyLineSegment { Points = pcol } }, IsClosed = false };
                }

            }
            else // no route defined
            {
                if (hasEpSource) UpdateSourceEpData(p1, p2);
                if (hasEpTarget)
                    p2 = p2.Subtract(UpdateTargetEpData(p2, p1));

                lineFigure = new PathFigure { StartPoint = p1, Segments = new PathSegmentCollection { new LineSegment() { Point = p2 } }, IsClosed = false };
            }
            ((PathGeometry)_linegeometry).Figures.Add(lineFigure);
#if WPF
            GeometryHelper.TryFreeze(lineFigure);
            GeometryHelper.TryFreeze(_linegeometry);
#endif
            if (ShowLabel && _edgeLabelControl != null && _updateLabelPosition && updateLabel)
                _edgeLabelControl.UpdatePosition();
        }

        private void UpdateSourceEpData(Point from, Point to)
        {
            var dir = MathHelper.GetDirection(from, to);
            _edgePointerForSource.Update(from, dir, _edgePointerForSource.NeedRotation ? -MathHelper.GetAngleBetweenPoints(from, to).ToDegrees() : 0);
        }

        private Point UpdateTargetEpData(Point from, Point to)
        {
            var dir = MathHelper.GetDirection(from, to);
            return _edgePointerForTarget.Update(from, dir, _edgePointerForTarget.NeedRotation ? (-MathHelper.GetAngleBetweenPoints(from, to).ToDegrees()) : 0);
        }

        #endregion

        /// <summary>
        /// Searches and returns template part by name if found
        /// </summary>
        /// <param name="name">Template PART name</param>
        /// <returns></returns>
        protected virtual object GetTemplatePart(string name)
        {
#if WPF
            return Template.FindName(name, this);
#elif METRO
            return this.FindDescendantByName(name);
#endif
        }

        public virtual SysRect GetLabelSize()
        {
            return _edgeLabelControl.GetSize();
        }

        public void SetCustomLabelSize(SysRect rect)
        {
            _edgeLabelControl.SetSize(rect);
        }

        internal virtual void UpdateLabelLayout()
        {
            _edgeLabelControl.Show();
            if (_edgeLabelControl.GetSize() == SysRect.Empty)

            {
                _edgeLabelControl.UpdateLayout();
                _edgeLabelControl.UpdatePosition();
            }
        }
    }
}
