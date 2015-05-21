using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using GraphX.Measure;
using GraphX.METRO.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using Point = Windows.Foundation.Point;
using Rect = GraphX.Measure.Rect;
using Size = Windows.Foundation.Size;
using Thickness = Windows.UI.Xaml.Thickness;

namespace GraphX.METRO.Controls
{
    /// <summary>
    /// Visual edge control
    /// </summary>
    [TemplatePart(Name = "PART_edgePath", Type = typeof(Path))]
    [TemplatePart(Name = "PART_edgeArrowPath", Type = typeof(Path))]
    [TemplatePart(Name = "PART_edgeLabel", Type = typeof(IEdgeLabelControl))]
    [TemplatePart(Name = "PART_EdgePointerForSource", Type = typeof(IEdgePointer))]
    [TemplatePart(Name = "PART_EdgePointerForTarget", Type = typeof(IEdgePointer))]
    public class EdgeControl : Control, IGraphControl, IDisposable
    {
        #region Dependency Properties


        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source",
                                                                                               typeof(VertexControl),
                                                                                               typeof(EdgeControl),
                                                                                               new PropertyMetadata(null));


        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target",
                                                                                               typeof(VertexControl),
                                                                                               typeof(EdgeControl),
                                                                                               new PropertyMetadata(null));

        public static readonly DependencyProperty EdgeProperty = DependencyProperty.Register("Edge", typeof(object),
                                                                                             typeof(EdgeControl),
                                                                                             new PropertyMetadata(null));

       ///!!! public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(typeof(EdgeControl), new PropertyMetadata(5.0));
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(Thickness),
                                                                                             typeof(EdgeControl),
                                                                                             new PropertyMetadata(5.0));


        /// <summary>
        /// Gets or sets parent GraphArea visual
        /// </summary>
        public GraphAreaBase RootArea
        {
            get { return (GraphAreaBase)GetValue(RootCanvasProperty); }
            set { SetValue(RootCanvasProperty, value); }
        }

        public static readonly DependencyProperty RootCanvasProperty =
            DependencyProperty.Register("RootArea", typeof(GraphAreaBase), typeof(EdgeControl), new PropertyMetadata(null));

        private static readonly DependencyProperty IsSelfLoopedProperty = DependencyProperty.Register("IsSelfLooped", typeof(bool), typeof(EdgeControl), new PropertyMetadata(false));

        private bool _isSelfLooped { get { return Source != null && Target != null && Source.Vertex == Target.Vertex; } }
        /// <summary>
        /// Gets if this edge is self looped (have same Source and Target)
        /// </summary>
        public bool IsSelfLooped
        {
            get { return _isSelfLooped; }
            protected set { SetValue(IsSelfLoopedProperty, value); }
        }

        #endregion

        #region Properties

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
                                                                                       typeof(EdgeControl),
                                                                                       new PropertyMetadata(EdgeDashStyle.Solid, dashstyle_changed));

        private static void dashstyle_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ec = d as EdgeControl;
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

        private DoubleCollection StrokeDashArray { get; set; }

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

        private bool _updateLabelPosition;
        /// <summary>
        /// Gets or sets if label position should be updated on edge update
        /// </summary>
        public bool UpdateLabelPosition { get { return _updateLabelPosition; } set { _updateLabelPosition = true; } }



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
                                                                               typeof(bool),
                                                                               typeof(EdgeControl),
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
        private bool _alignLabelsToEdges;

        public static readonly DependencyProperty LabelVerticalOffsetProperty = DependencyProperty.Register("LabelVerticalOffset",
                                                                               typeof(double),
                                                                               typeof(EdgeControl),
                                                                               new PropertyMetadata(0));

        
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
        private Geometry _linegeometry;

        /// <summary>
        /// Geometry object that represents visual edge arrow. Applied in OnApplyTemplate and OnRender.
        /// </summary>
        private PathGeometry _arrowgeometry;

        /// <summary>
        /// Templated Path object to operate with routed path
        /// </summary>
        private Path _linePathObject;
        /// <summary>
        /// Templated Path object to operate with routed path arrow head
        /// </summary>
        private Path _arrowPathObject;

        /// <summary>
        /// Templated label control to display labels
        /// </summary>
        private IEdgeLabelControl _edgeLabelControl;


        public EdgeEventOptions EventOptions { get; private set; }

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

        /// <summary>
        /// Custom edge thickness
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        private IEdgePointer _edgePointerForSource;
        private IEdgePointer _edgePointerForTarget;
        #endregion

        #region public Clean()
        public void Clean()
        {
            if (_sourceWatcher != null)
                _sourceWatcher.Dispose();
            if (_targetWatcher != null)
                _targetWatcher.Dispose();
            if (Source != null)
                Source.PositionChanged -= source_PositionChanged;
            if (Target != null)
                Target.PositionChanged -= source_PositionChanged;
            _oldSource = _oldTarget = null;
            Source = null;
            Target = null;
            Edge = null;
            RootArea = null;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            _linegeometry = null;
            _arrowgeometry = null;
            _linePathObject = null;
            _arrowPathObject = null;
            if (_edgeLabelControl != null)
            {
                _edgeLabelControl.Dispose();
                _edgeLabelControl = null;
            }

            if (_edgePointerForSource != null)
            {
                _edgePointerForSource.Dispose();
                _edgePointerForSource = null;
            }
            if (_edgePointerForTarget != null)
            {
                _edgePointerForTarget.Dispose();
                _edgePointerForTarget = null;
            }
            if (EventOptions != null)
                EventOptions.Clean();
        }
        #endregion

        public EdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true)
        {
            DefaultStyleKey = typeof(EdgeControl);
            DataContext = edge;
            Source = source; Target = target;
            Edge = edge; DataContext = edge;
            ShowArrows = showArrows;
            ShowLabel = showLabels;
            _updateLabelPosition = true;
            IsHiddenEdgesUpdated = true;

            EventOptions = new EdgeEventOptions(this);
            foreach (var item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
                UpdateEventhandling(item);

            TargetChanged(null, null);
            SourceChanged(null, null);
            _sourceWatcher = this.WatchProperty("Source", SourceChanged);
            _targetWatcher = this.WatchProperty("Target", TargetChanged);
        }

        public EdgeControl()
            : this(null, null, null)
        {
        }

        #region Position tracing

        private void TargetChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (_oldTarget != null)
                _oldTarget.PositionChanged -= source_PositionChanged;
            _oldTarget = Target;
            if (Target != null)
                Target.PositionChanged += source_PositionChanged;
            IsSelfLooped = _isSelfLooped;
        }

        private void SourceChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (_oldSource != null)
                _oldSource.PositionChanged -= source_PositionChanged;
            _oldSource = Source;

            if (Source != null)
                Source.PositionChanged += source_PositionChanged;
            IsSelfLooped = _isSelfLooped;
        }

        void IPositionChangeNotify.OnPositionChanged()
        {
            //skip any actions on own position change
        }

        private void source_PositionChanged(object sender, EventArgs e)
        {
            //update edge on any connected vertex position changes
            UpdateEdge();
        }

        private IDisposable _sourceWatcher;
        private IDisposable _targetWatcher;
        private VertexControl _oldSource;
        private VertexControl _oldTarget;
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

        #region Event handlers

        internal void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled) PointerPressed += GraphEdge_MouseDown;
                    else PointerPressed -= GraphEdge_MouseDown;
                    break;
                case EventType.MouseDoubleClick:
                    //if (EventOptions.MouseDoubleClickEnabled) MouseDoubleClick += EdgeControl_MouseDoubleClick;
                    //else MouseDoubleClick -= EdgeControl_MouseDoubleClick;
                    break;
                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled) PointerEntered += EdgeControl_MouseEnter;
                    else PointerEntered -= EdgeControl_MouseEnter;
                    break;
                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled) PointerExited += EdgeControl_MouseLeave;
                    else PointerExited -= EdgeControl_MouseLeave;
                    break;

                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled) PointerMoved += EdgeControl_MouseMove;
                    else PointerMoved -= EdgeControl_MouseMove;
                    break;
            }
        }

        void EdgeControl_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseLeave(this, e);
            // e.Handled = true;
        }

        void EdgeControl_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseEnter(this, e);
            // e.Handled = true;
        }

        void EdgeControl_MouseMove(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseMove(this, e);
            e.Handled = true;
        }

        void EdgeControl_MouseDoubleClick(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeDoubleClick(this, e);
            e.Handled = true;
        }

        void GraphEdge_MouseDown(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeSelected(this, e);
            e.Handled = true;
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

        internal void InvalidateChildren()
        {
            if (_edgeLabelControl != null)
                _edgeLabelControl.UpdateLayout();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null)
            {
                //!!! use GetTemplateChild() -???
                //check findname
                _linePathObject = this.FindDescendantByName("PART_edgePath") as Path;
                if (_linePathObject == null) throw new GX_ObjectNotFoundException("EdgeControl Template -> Edge template must contain 'PART_edgePath' Path object to draw route points!");
                _linePathObject.Data = _linegeometry;
                _arrowPathObject = this.FindDescendantByName("PART_edgeArrowPath") as Path;
                if (_arrowPathObject != null)
                    _arrowPathObject.Data = _arrowgeometry;

                _edgeLabelControl = this.FindDescendantByName("PART_edgeLabel") as IEdgeLabelControl;

                _edgePointerForSource = this.FindDescendantByName("PART_EdgePointerForSource") as IEdgePointer;
                _edgePointerForTarget = this.FindDescendantByName("PART_EdgePointerForTarget") as IEdgePointer;

                //if (EdgeLabelControl == null) Debug.WriteLine("EdgeControl Template -> Edge template have no 'PART_edgeLabel' object to draw!");
                UpdateEdge();
            }

        }

        #region public PrepareEdgePath()


        internal void UpdateEdge(bool updateLabel = true)
        {
            if (Visibility == Visibility.Visible || IsHiddenEdgesUpdated)
            {
                if (_linePathObject == null)
                    ApplyTemplate();
                PrepareEdgePath(true, null, updateLabel);
                if (_linePathObject == null) return;
                _linePathObject.Data = _linegeometry;
                _linePathObject.StrokeDashArray = StrokeDashArray;

                if (_arrowPathObject != null)
                    _arrowPathObject.Data = ShowArrows ? _arrowgeometry : null;
                if (_edgeLabelControl != null)
                    if(ShowLabel) _edgeLabelControl.Show(); else _edgeLabelControl.Hide();
            }
        }

        internal int SourceOffset;
        internal int TargetOffset;

        /// <summary>
        /// Gets the offset point for edge parallelization
        /// </summary>
        /// <param name="source">Source vertex</param>
        /// <param name="target">Target vertex</param>
        /// <param name="sideDistance">Distance between edges</param>
        internal Point GetParallelOffset(VertexControl source, VertexControl target, int sideDistance)
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
        /// Create and apply edge path using calculated ER parameters stored in edge
        /// </summary>
        /// <param name="useCurrentCoords">Use current vertices coordinates or final coorfinates (for.ex if move animation is active final coords will be its destination)</param>
        /// <param name="externalRoutingPoints">Provided custom routing points will be used instead of stored ones.</param>
        /// <param name="updateLabel">Should edge label be updated in this pass</param>
        public void PrepareEdgePath(bool useCurrentCoords = false, Measure.Point[] externalRoutingPoints = null, bool updateLabel = true)
        {
            //do not calculate invisible edges
            if ((Visibility != Visibility.Visible && !IsHiddenEdgesUpdated) && Source == null || Target == null || ManualDrawing) return;

            var template = Template;
            if (template != null)
            {
                #region Get the inputs
                //get the size of the source
                var sourceSize = new Size
                {
                    Width = Source.ActualWidth,
                    Height = Source.ActualHeight
                };
                if (DesignMode.DesignModeEnabled) sourceSize = new Size(80, 20);

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
                if (DesignMode.DesignModeEnabled) 
                    targetSize = new Size(80, 20);

                //get the position center of the target
                var targetPos = new Point
                {
                    X = (useCurrentCoords ? GraphAreaBase.GetX(Target) : GraphAreaBase.GetFinalX(Target)) + targetSize.Width * .5,
                    Y = (useCurrentCoords ? GraphAreaBase.GetY(Target) : GraphAreaBase.GetFinalY(Target)) + targetSize.Height * .5
                };

                var routedEdge = Edge as IRoutingInfo;
                if(routedEdge == null)
                    throw new GX_InvalidDataException("Edge must implement IRoutingInfo interface");

                //get the route informations
                var routeInformation = externalRoutingPoints ?? routedEdge.RoutingPoints;
                #endregion

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

                //if self looped edge
                if (IsSelfLooped)
                {
                    if (!RootArea.EdgeShowSelfLooped) return;
                    var pt = new Point(sourcePos1.X + RootArea.EdgeSelfLoopCircleOffset.X - RootArea.EdgeSelfLoopCircleRadius, sourcePos1.Y + RootArea.EdgeSelfLoopCircleOffset.X - RootArea.EdgeSelfLoopCircleRadius);
                    var geo = new EllipseGeometry {Center = pt, RadiusX = RootArea.EdgeSelfLoopCircleRadius, RadiusY = RootArea.EdgeSelfLoopCircleRadius};

                    const double dArrowAngle = Math.PI / 2.0;
                    _arrowgeometry = new PathGeometry();
                    var aPoint = sourcePos1;
                    _arrowgeometry.Figures.Add(GeometryHelper.GenerateArrow(aPoint, new Point(), new Point(), dArrowAngle));
                    _linegeometry = geo;

                    if (hasEpSource)
                        _edgePointerForSource.Hide();
                    if (hasEpTarget)
                        _edgePointerForTarget.Hide();
                    return;
                }


                var hasRouteInfo = routeInformation != null && routeInformation.Length > 1;

                //calculate source and target edge attach points
                if (RootArea != null && !hasRouteInfo && RootArea.EnableParallelEdges)
                {
                    if (SourceOffset != 0) sourcePos = GetParallelOffset(Source, Target, SourceOffset);
                    if (TargetOffset != 0) targetPos = GetParallelOffset(Target, Source, TargetOffset);
                }

                /* Rectangular shapes implementation by bleibold */

                var gEdge = Edge as IGraphXCommonEdge;
                Point p1;
                Point p2;

                if (gEdge != null && gEdge.SourceConnectionPointId.HasValue)
                {
                    var sourceCp = Source.GetConnectionPointById(gEdge.SourceConnectionPointId.Value, true);
                    if (sourceCp.Shape == VertexShape.None) p1 = sourceCp.RectangularSize.Center();
                    else
                    {
                        var targetCpPos = gEdge.TargetConnectionPointId.HasValue ? Target.GetConnectionPointById(gEdge.TargetConnectionPointId.Value, true).RectangularSize.Center() : (hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos));
                        p1 = GeometryHelper.GetEdgeEndpoint(sourceCp.RectangularSize.Center(), sourceCp.RectangularSize, targetCpPos, sourceCp.Shape);
                    }
                }
                else
                    p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new Windows.Foundation.Rect(sourcePos1, sourceSize), (hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos)), Source.VertexShape);

                if (gEdge != null && gEdge.TargetConnectionPointId.HasValue)
                {
                    var targetCp = Target.GetConnectionPointById(gEdge.TargetConnectionPointId.Value, true);
                    if (targetCp.Shape == VertexShape.None) p2 = targetCp.RectangularSize.Center();
                    else
                    {
                        var sourceCpPos = gEdge.SourceConnectionPointId.HasValue ? Source.GetConnectionPointById(gEdge.SourceConnectionPointId.Value, true).RectangularSize.Center() : hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos);
                        p2 = GeometryHelper.GetEdgeEndpoint(targetCp.RectangularSize.Center(), targetCp.RectangularSize, sourceCpPos, targetCp.Shape);
                    }
                }
                else
                    p2 = GeometryHelper.GetEdgeEndpoint(targetPos, new Windows.Foundation.Rect(targetPos1, targetSize), hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos), Target.VertexShape);

                SourceConnectionPoint = p1;
                TargetConnectionPoint = p2;

                _linegeometry = new PathGeometry(); PathFigure lineFigure;
                //TODO clear in 2.2.0 in favor of new arrow path logic
                _arrowgeometry = new PathGeometry(); PathFigure arrowFigure;

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
                        //get two last points of curved path to generate correct arrow
                        var cLast = oPolyLineSegment.Points.Last();
                        var cPrev = oPolyLineSegment.Points.Count == 1 ? oPolyLineSegment.Points.Last() : oPolyLineSegment.Points[oPolyLineSegment.Points.Count - 2];
                        arrowFigure = GeometryHelper.GenerateOldArrow(cPrev, cLast);
                    }
                    else
                    {
                        if (hasEpSource) UpdateSourceEpData(routePoints.First(), routePoints[1]);
                        if (hasEpTarget)
                            routePoints[routePoints.Count - 1] = routePoints[routePoints.Count - 1].Subtract(UpdateTargetEpData(p2, routePoints[routePoints.Count - 2]));

                        var pcol = new PointCollection();
                        foreach(var item in routePoints)
                            pcol.Add(item);

                        lineFigure = new PathFigure {StartPoint = p1, Segments = new PathSegmentCollection {new PolyLineSegment {Points = pcol}}, IsClosed = false};
                        arrowFigure = GeometryHelper.GenerateOldArrow(routePoints[routePoints.Count - 2], p2);
                    }

                }
                else // no route defined
                {
                    if (hasEpSource) UpdateSourceEpData(p1, p2);
                    if (hasEpTarget)
                        p2 = p2.Subtract(UpdateTargetEpData(p2, p1));

                    lineFigure = new PathFigure {StartPoint = p1, Segments = new PathSegmentCollection {new LineSegment() {Point = p2}}, IsClosed = false};
                    arrowFigure = GeometryHelper.GenerateOldArrow(p1, p2);
                }
                ((PathGeometry) _linegeometry).Figures.Add(lineFigure);
                if (arrowFigure != null)
                    _arrowgeometry.Figures.Add(arrowFigure);
                if (ShowLabel && _edgeLabelControl != null && _updateLabelPosition && updateLabel)
                    _edgeLabelControl.UpdatePosition();
            }
            else
            {
                Debug.WriteLine("PrepareEdgePath() -> Edge template not found! Can't apply path to display edge!");
            }

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

        public void Dispose()
        {
            Clean();
        }

        public Rect GetLabelSize()
        {
            return _edgeLabelControl.GetSize();
        }

        public void SetCustomLabelSize(Windows.Foundation.Rect rect)
        {
           _edgeLabelControl.SetSize(rect);
        }

        internal void UpdateLabelLayout()
        {
            _edgeLabelControl.Show();
            if (_edgeLabelControl.GetSize() == Rect.Empty)
            {
                _edgeLabelControl.UpdateLayout();
                _edgeLabelControl.UpdatePosition();
            }
        }
    }
}