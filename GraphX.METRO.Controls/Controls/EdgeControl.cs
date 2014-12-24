using System;
using System.Linq;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using GraphX.METRO.Controls;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual edge control
    /// </summary>
    [TemplatePart(Name = "PART_edgePath", Type = typeof(Path))]
    [TemplatePart(Name = "PART_edgeArrowPath", Type = typeof(Path))]
    [TemplatePart(Name = "PART_edgeLabel", Type = typeof(EdgeLabelControl))]
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


        /// <summary>
        /// Show arrows on the edge ends. Default value is true.
        /// </summary>
        public bool ShowArrows { get { return _showarrows; } set { _showarrows = value; UpdateEdge(false); } }
        private bool _showarrows;


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

        /// <summary>
        /// Offset for labels Y axis to display it above/below the edge
        /// </summary>
        public double LabelVerticalOffset { get; set; }

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
        private EdgeLabelControl _edgeLabelControl;


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
                if (_arrowPathObject == null) Debug.WriteLine("EdgeControl Template -> Edge template have no 'PART_edgeArrowPath' Path object to draw!");
                else
                {
                    _arrowPathObject.Data = _arrowgeometry;
                }
                _edgeLabelControl = this.FindDescendantByName("PART_edgeLabel") as EdgeLabelControl;
                //if (EdgeLabelControl == null) Debug.WriteLine("EdgeControl Template -> Edge template have no 'PART_edgeLabel' object to draw!");
                UpdateEdge();
            }

        }

        /*protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
           // if(_geometry!=null)
            //    drawingContext.DrawGeometry(new SolidColorBrush(Colors.Black), new Pen(new SolidColorBrush(Colors.Black), 2), _geometry);
        }*/



        #region public PrepareEdgePath()


        internal void UpdateEdge(bool updateLabel = true)
        {
            if ((Visibility == Visibility.Visible || IsHiddenEdgesUpdated) && _linePathObject != null)
            {
                PrepareEdgePath(true, null, updateLabel);
                _linePathObject.Data = _linegeometry;
                _linePathObject.StrokeDashArray = StrokeDashArray;

                if (_arrowPathObject != null)
                    _arrowPathObject.Data = ShowArrows ? _arrowgeometry : null;
                if (_edgeLabelControl != null)
                    _edgeLabelControl.Visibility = ShowLabel ? Visibility.Visible : Visibility.Collapsed;
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

            var mainVector = new Measure.Vector(targetpos.X - sourcepos.X, targetpos.Y - sourcepos.Y);
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
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) sourceSize = new Size(80, 20);

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
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) 
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


                //Point p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new Rect(sourceSize), (hasRouteInfo ? routeInformation[1] : (targetPos)), Source.MathShape);
                //Point p2 = GeometryHelper.GetEdgeEndpoint(targetPos, new Rect(targetSize), hasRouteInfo ? routeInformation[routeInformation.Length - 2] : (sourcePos), Target.MathShape);

                var p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new Rect(sourcePos1, sourceSize), (hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos)), Source.MathShape);
                var p2 = GeometryHelper.GetEdgeEndpoint(targetPos, new Rect(targetPos1, targetSize), hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos), Target.MathShape);

                SourceConnectionPoint = p1;
                TargetConnectionPoint = p2;

                _linegeometry = new PathGeometry(); PathFigure lineFigure;
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
                        lineFigure = GeometryHelper.GetPathFigureFromPathSegments(routePoints[0], true, true, oPolyLineSegment);
                        //get two last points of curved path to generate correct arrow
                        var cLast = oPolyLineSegment.Points.Last();
                        var cPrev = oPolyLineSegment.Points.Count == 1 ? oPolyLineSegment.Points.Last() : oPolyLineSegment.Points[oPolyLineSegment.Points.Count - 2];
                        arrowFigure = GeometryHelper.GenerateOldArrow(cPrev, cLast);
                        //freeze and create resulting geometry
                    }
                    else
                    {
                        var pcol = new PointCollection();
                        foreach(var item in routePoints)
                            pcol.Add(item);

                        lineFigure = new PathFigure {StartPoint = p1, Segments = new PathSegmentCollection {new PolyLineSegment {Points = pcol}}, IsClosed = false};
                        arrowFigure = GeometryHelper.GenerateOldArrow(routePoints[routePoints.Count - 2], p2);
                    }

                }
                else // no route defined
                {
                    //!!! Here is the line calculation to not overlap an arrowhead
                    //Vector v = p1 - p2; v = v / v.Length * 5;
                    // Vector n = new Vector(-v.Y, v.X) * 0.7;
                    //segments[0] = new LineSegment(p2 + v, true);
                    lineFigure = new PathFigure {StartPoint = p1, Segments = new PathSegmentCollection {new LineSegment() {Point = p2}}, IsClosed = false};
                    arrowFigure = GeometryHelper.GenerateOldArrow(p1, p2);
                }
                //GeometryHelper.TryFreeze(lineFigure);
                (_linegeometry as PathGeometry).Figures.Add(lineFigure);
                if (arrowFigure != null)
                {
                   // GeometryHelper.TryFreeze(arrowFigure);
                    _arrowgeometry.Figures.Add(arrowFigure);
                }
                //GeometryHelper.TryFreeze(_linegeometry);
                //GeometryHelper.TryFreeze(_arrowgeometry);

                if (ShowLabel && _edgeLabelControl != null && _updateLabelPosition && updateLabel)
                    _edgeLabelControl.UpdatePosition();
                //PathGeometry = (PathGeometry)_linegeometry;
            }
            else
            {
                Debug.WriteLine("PrepareEdgePath() -> Edge template not found! Can't apply path to display edge!");
            }

        }
        #endregion

        public void Dispose()
        {
            Clean();
        }

        public Measure.Rect GetLabelSize()
        {
            return _edgeLabelControl.LastKnownRectSize.ToGraphX();
        }

        public void SetCustomLabelSize(Rect rect)
        {
            _edgeLabelControl.LastKnownRectSize = rect;
            _edgeLabelControl.Arrange(rect);
        }

        internal void UpdateLabelLayout()
        {
            _edgeLabelControl.Visibility = Visibility.Visible;
            if (_edgeLabelControl.LastKnownRectSize == Rect.Empty || double.IsNaN(_edgeLabelControl.Width))
            {
                _edgeLabelControl.UpdateLayout();
                _edgeLabelControl.UpdatePosition();
            }
        }
    }
}