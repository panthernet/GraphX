﻿using System;
using System.Linq;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using System.Windows.Input;

#if WPF

using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using SysRect = System.Windows.Rect;

#elif METRO
using GraphX.PCL.Common;
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

        /// <summary>
        /// Gets or sets if edge pointer should be hidden when source and target vertices are overlapped making the 0 length edge. Default value is True.
        /// </summary>
        public bool HideEdgePointerOnVertexOverlap { get; set; } = true;

        /// <summary>
        /// Gets or sets the length of the edge to hide the edge pointers if less than or equal to. Default value is 0 (do not hide).
        /// </summary>
        public double HideEdgePointerByEdgeLength { get; set; } = 0.0d;

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

        protected virtual void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        protected virtual void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Gets or sets parent GraphArea visual
        /// </summary>
        public GraphAreaBase RootArea
        {
            get { return (GraphAreaBase)GetValue(RootCanvasProperty); }
            set { SetValue(RootCanvasProperty, value); }
        }

        public static readonly DependencyProperty RootCanvasProperty =
            DependencyProperty.Register(nameof(RootArea), typeof(GraphAreaBase), typeof(EdgeControlBase), new PropertyMetadata(null));

        public static readonly DependencyProperty SelfLoopIndicatorRadiusProperty = DependencyProperty.Register(nameof(SelfLoopIndicatorRadius),
                                                                                       typeof(double),
                                                                                       typeof(EdgeControlBase),
                                                                                       new PropertyMetadata(5d));

        /// <summary>
        /// Radius of the default self-loop indicator, which is drawn as a circle (when custom template isn't provided). Default is 20.
        /// </summary>
        public double SelfLoopIndicatorRadius
        {
            get { return (double)GetValue(SelfLoopIndicatorRadiusProperty); }
            set { SetValue(SelfLoopIndicatorRadiusProperty, value); }
        }

        public static readonly DependencyProperty SelfLoopIndicatorOffsetProperty = DependencyProperty.Register(nameof(SelfLoopIndicatorOffset),
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

        public static readonly DependencyProperty ShowSelfLoopIndicatorProperty = DependencyProperty.Register(nameof(ShowSelfLoopIndicator),
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

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source),
                                                                                               typeof(VertexControl),
                                                                                               typeof(EdgeControlBase),
                                                                                               new PropertyMetadata(null, OnSourceChangedInternal));

        private static void OnSourceChangedInternal(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EdgeControlBase)?.OnSourceChanged(d, e);
        }

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target),
                                                                                               typeof(VertexControl),
                                                                                               typeof(EdgeControlBase),
                                                                                               new PropertyMetadata(null, OnTargetChangedInternal));

        private static void OnTargetChangedInternal(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EdgeControlBase)?.OnTargetChanged(d, e);
        }

        public static readonly DependencyProperty EdgeProperty = DependencyProperty.Register(nameof(Edge), typeof(object),
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
                return EdgeLabelControl?.Angle ?? _labelAngle;
            }
            set
            {
                _labelAngle = value;
                if (EdgeLabelControl != null) EdgeLabelControl.Angle = _labelAngle;
            }
        }

        #region DashStyle

        public static readonly DependencyProperty DashStyleProperty = DependencyProperty.Register(nameof(DashStyle),
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

        #endregion DashStyle

        /// <summary>
        /// Gets or sets if this edge can be paralellized if GraphArea.EnableParallelEdges is true.
        /// If not it will be drawn by default.
        /// </summary>
        public bool CanBeParallel { get; set; } = true;

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
        protected PropertyChangeNotifier _sourceWatcher;
        protected PropertyChangeNotifier _targetWatcher;
#endif

        /// <summary>
        /// Gets or set if hidden edges should be updated when connected vertices positions are changed. Default value is True.
        /// </summary>
        public bool IsHiddenEdgesUpdated { get; set; }

        public static readonly DependencyProperty ShowArrowsProperty = DependencyProperty.Register("ShowArrows", typeof(bool), typeof(EdgeControlBase), new PropertyMetadata(true, showarrows_changed));

        private static void showarrows_changed(object sender, DependencyPropertyChangedEventArgs args)
        {
            var ctrl = sender as EdgeControlBase;
            if (ctrl == null)
                return;

            if (ctrl.EdgePointerForSource != null && !ctrl.IsSelfLooped)
                if (ctrl.ShowArrows) ctrl.EdgePointerForSource.Show(); else ctrl.EdgePointerForSource.Hide();
            if (ctrl.EdgePointerForTarget != null && !ctrl.IsSelfLooped)
                if (ctrl.ShowArrows) ctrl.EdgePointerForTarget.Show(); else ctrl.EdgePointerForTarget.Hide();
            ctrl.UpdateEdge(false);
        }

        /// <summary>
        /// Show arrows on the edge ends. Default value is true.
        /// </summary>
        public bool ShowArrows { get { return (bool)GetValue(ShowArrowsProperty); } set { SetValue(ShowArrowsProperty, value); } }

        public static readonly DependencyProperty ShowLabelProperty = DependencyProperty.Register("ShowLabel",
                                                                               typeof(bool),
                                                                               typeof(EdgeControlBase),
                                                                               new PropertyMetadata(false, showlabel_changed));

        private static void showlabel_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EdgeControlBase)?.UpdateEdge();
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
                if (EdgeLabelControl != null)
                {
                    if (value == false) EdgeLabelControl.Angle = 0;
                    EdgeLabelControl.UpdatePosition();
                }
            }
        }

        protected bool _alignLabelsToEdges;

        public static readonly DependencyProperty LabelVerticalOffsetProperty = DependencyProperty.Register("LabelVerticalOffset",
                                                                               typeof(double),
                                                                               typeof(EdgeControlBase),
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
        protected Geometry Linegeometry;

        /// <summary>
        /// Templated Path object to operate with routed path
        /// </summary>
        protected Path LinePathObject;

        private IEdgeLabelControl _edgeLabelControl;

        /// <summary>
        /// Templated label control to display labels
        /// </summary>
        protected internal IEdgeLabelControl EdgeLabelControl { get { return _edgeLabelControl; } set { _edgeLabelControl = value; OnEdgeLabelUpdated(); } }

        protected IEdgePointer EdgePointerForSource;
        protected IEdgePointer EdgePointerForTarget;

        /// <summary>
        /// Returns edge pointer for source if any
        /// </summary>
        public IEdgePointer GetEdgePointerForSource() { return EdgePointerForSource; }

        /// <summary>
        /// Returns edge pointer for target if any
        /// </summary>
        public IEdgePointer GetEdgePointerForTarget() { return EdgePointerForTarget; }

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

        /// <summary>
        /// Internal method. Attaches label to control
        /// </summary>
        /// <param name="ctrl"></param>
        public void AttachLabel(IEdgeLabelControl ctrl)
        {
            EdgeLabelControl = ctrl;
            UpdateLabelLayout();
        }

        /// <summary>
        /// Internal method. Detaches label from control.
        /// </summary>
        public void DetachLabel()
        {
            (EdgeLabelControl as IAttachableControl<EdgeControl>)?.Detach();
            EdgeLabelControl = null;
        }

        /// <summary>
        /// Update edge label if any
        /// </summary>
        public void UpdateLabel()
        {
            if (_edgeLabelControl != null && ShowLabel)
                UpdateLabelLayout(true);
        }

        #endregion Properties & Fields

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

        #endregion Position methods

        #region Manual path controls

        /// <summary>
        /// Gets current edge path geometry object
        /// </summary>
        public PathGeometry GetEdgePathManually()
        {
            if (!ManualDrawing) return null;
            return Linegeometry as PathGeometry;
        }

        /// <summary>
        /// Sets current edge path geometry object
        /// </summary>
        public void SetEdgePathManually(PathGeometry geo)
        {
            if (!ManualDrawing) return;
            Linegeometry = geo;
            UpdateEdge();
        }

        #endregion Manual path controls

        internal void SetVisibility(Visibility value)
        {
            this.SetCurrentValue(VisibilityProperty, value);
        }

        internal virtual void InvalidateChildren()
        {
            EdgeLabelControl?.UpdateLayout();
            if (LinePathObject != null)
            {
                var pos = this.Source.GetPosition();
                this.Source.SetPosition(pos.X, pos.Y);
            }
        }

        /// <summary>
        /// Gets if Template has been loaded and edge can operate at 100%
        /// </summary>
        public bool IsTemplateLoaded => LinePathObject != null;

        protected virtual void OnEdgeLabelUpdated()
        {
        }

#if WPF

        public override void OnApplyTemplate()
#elif METRO
		protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();

            if (Template == null) return;

            LinePathObject = GetTemplatePart("PART_edgePath") as Path;
            if (LinePathObject == null) throw new GX_ObjectNotFoundException("EdgeControlBase Template -> Edge template must contain 'PART_edgePath' Path object to draw route points!");
            LinePathObject.Data = Linegeometry;
            if (this.FindDescendantByName("PART_edgeArrowPath") != null)
                throw new GX_ObsoleteException("PART_edgeArrowPath is obsolete! Please use new DefaultEdgePointer object in your EdgeControlBase template!");

            EdgeLabelControl = EdgeLabelControl ?? GetTemplatePart("PART_edgeLabel") as IEdgeLabelControl;

            EdgePointerForSource = GetTemplatePart("PART_EdgePointerForSource") as IEdgePointer;
            EdgePointerForTarget = GetTemplatePart("PART_EdgePointerForTarget") as IEdgePointer;

            SelfLoopIndicator = GetTemplatePart("PART_SelfLoopedEdge") as FrameworkElement;
            if (SelfLoopIndicator != null)
                SelfLoopIndicator.LayoutUpdated += (sender, args) =>
                {
                    SelfLoopIndicator?.Arrange(_selfLoopedEdgeLastKnownRect);
                };
            // var x = ShowLabel;
            MeasureChild(EdgePointerForSource as UIElement);
            MeasureChild(EdgePointerForTarget as UIElement);
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
            child?.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
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
                //first show label to get DesiredSize
                if (EdgeLabelControl != null)
                    if (ShowLabel) EdgeLabelControl.Show(); else EdgeLabelControl.Hide();
                UpdateEdgeRendering(updateLabel);
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
            if (LinePathObject == null) return;
            LinePathObject.Data = Linegeometry;
            LinePathObject.StrokeDashArray = StrokeDashArray;
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
        protected internal Point? SourceConnectionPoint;

        /// <summary>
        /// Internal value to store last calculated Target vertex connection point
        /// </summary>
        protected internal Point? TargetConnectionPoint;

        /// <summary>
        ///Gets is looped edge indicator template available. Used to pass some heavy cycle checks.
        /// </summary>
        protected bool HasSelfLoopedEdgeTemplate => SelfLoopIndicator != null;

        /// <summary>
        /// Update SLE data such as template, edge pointers visibility
        /// </summary>
        protected virtual void UpdateSelfLoopedEdgeData()
        {
            //generate object if template is present
            if (IsSelfLooped)
            {
                //hide edge pointers
                EdgePointerForSource?.Hide();
                EdgePointerForTarget?.Hide();

                //return if we don't need to show edge loops
                if (!ShowSelfLoopIndicator) return;

                //pregenerate built-in indicator geometry if template PART is absent
                if (!HasSelfLoopedEdgeTemplate)
                    Linegeometry = new EllipseGeometry();
                else SelfLoopIndicator.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            }
            else
            {
                //if (_edgePointerForSource != null && ShowArrows) _edgePointerForSource.Show();
                //if (_edgePointerForTarget != null && ShowArrows) _edgePointerForTarget.Show();

                if (HasSelfLoopedEdgeTemplate)
                    SelfLoopIndicator.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
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
                    sourcePos.Y + SelfLoopIndicatorOffset.Y - (hasNoTemplate ? SelfLoopIndicatorRadius : SelfLoopIndicator.DesiredSize.Height));

            //if we has no self looped edge template defined we'll use default built-in indicator
            if (hasNoTemplate)
            {
                //var geometry = Linegeometry as EllipseGeometry;
                //TODO
                //geometry.Center = pt;
                //geometry.RadiusX = SelfLoopIndicatorRadius;
                //geometry.RadiusY = SelfLoopIndicatorRadius;
            }
            else _selfLoopedEdgeLastKnownRect = new SysRect(pt, SelfLoopIndicator.DesiredSize);
        }

        public virtual void PrepareEdgePathFromMousePointer(bool useCurrentCoords = false)
        {
            //do not calculate invisible edges
            if ((Visibility != Visibility.Visible && !IsHiddenEdgesUpdated) && ManualDrawing || !IsTemplateLoaded) return;

            //get the size of the source
            var sourceSize = new Size
            {
                Width = this.Source.ActualWidth,
                Height = this.Source.ActualHeight
            };

            //get the position center of the source
            var sourcePos = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(this.Source) : GraphAreaBase.GetFinalX(this.Source)) + sourceSize.Width * 0.5,
                Y = (useCurrentCoords ? GraphAreaBase.GetY(this.Source) : GraphAreaBase.GetFinalY(this.Source)) + sourceSize.Height * 0.5
            };

            //get the size of the target
            var targetSize = new Size
            {
                Width = SystemParameters.CursorWidth,
                Height = SystemParameters.CursorHeight
            };

            //get the position center of the target
            var targetPos = new Point
            {
                X = Mouse.GetPosition(this.RootArea).X,/* + targetSize.Width * 0.5,*/
                Y = Mouse.GetPosition(this.RootArea).Y/* + targetSize.Height * 0.5*/
            };

            var routedEdge = this.Edge as IRoutingInfo;

            if (routedEdge == null)
                throw new GX_InvalidDataException("Edge must implement IRoutingInfo interface");

            //get the route informations
            var routeInformation = routedEdge.RoutingPoints;

            // Get the TopLeft position of the Source Vertex.
            var sourcePos1 = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(this.Source) : GraphAreaBase.GetFinalX(this.Source)),
                Y = (useCurrentCoords ? GraphAreaBase.GetY(this.Source) : GraphAreaBase.GetFinalY(this.Source))
            };
            // Get the TopLeft position of the Target Vertex.
            var targetPos1 = new Point
            {
                X = Mouse.GetPosition(this.RootArea).X,
                Y = Mouse.GetPosition(this.RootArea).Y
            };

            var hasEpSource = EdgePointerForSource != null;
            var hasEpTarget = EdgePointerForTarget != null;

            //if self looped edge
            if (IsSelfLooped)
            {
                PrepareSelfLoopedEdge(sourcePos1);
                return;
            }

            //check if we have some edge route data
            var hasRouteInfo = routeInformation != null && routeInformation.Length > 1;

            var gEdge = Edge as IGraphXCommonEdge;
            Point p1;
            Point p2;

            //calculate edge source (p1) and target (p2) endpoints based on different settings
            if (gEdge?.SourceConnectionPointId != null)
            {
                var sourceCp = this.Source.GetConnectionPointById(gEdge.SourceConnectionPointId.Value, true);
                if (sourceCp == null)
                {
                    throw new GX_ObjectNotFoundException(string.Format("Can't find source vertex VCP by edge source connection point Id({1}) : {0}", this.Source, gEdge.SourceConnectionPointId));
                }
                if (sourceCp.Shape == VertexShape.None) p1 = sourceCp.RectangularSize.Center();
                else
                {
                    var targetCpPos = hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos);
                    p1 = GeometryHelper.GetEdgeEndpoint(sourceCp.RectangularSize.Center(), sourceCp.RectangularSize, targetCpPos, sourceCp.Shape);
                }
            }
            else
                p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new SysRect(sourcePos1, sourceSize), (hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos)), this.Source.VertexShape);

            //if (gEdge?.TargetConnectionPointId != null)
            //{
            //    //var targetCp = this.Target.GetConnectionPointById(gEdge.TargetConnectionPointId.Value, true);
            //    //if (targetCp == null)
            //    //    throw new GX_ObjectNotFoundException(string.Format("Can't find target vertex VCP by edge target connection point Id({1}) : {0}", this.Target, gEdge.TargetConnectionPointId));
            //    //if (targetCp.Shape == VertexShape.None) p2 = targetCp.RectangularSize.Center();
            //    //else
            //    //{
            //    //    var sourceCpPos = gEdge.SourceConnectionPointId.HasValue ? this.Source.GetConnectionPointById(gEdge.SourceConnectionPointId.Value, true).RectangularSize.Center() : hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos);
            //    //    p2 = GeometryHelper.GetEdgeEndpoint(targetCp.RectangularSize.Center(), targetCp.RectangularSize, sourceCpPos, targetCp.Shape);
            //    //}
            //}
            //else
            p2 = GeometryHelper.GetEdgeEndpoint(
                targetPos, new SysRect(targetPos, targetSize), hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos), VertexShape.None);

            SourceConnectionPoint = p1;
            TargetConnectionPoint = p2;

            Linegeometry = new PathGeometry();
            PathFigure lineFigure = new PathFigure();

            //if we have route and route consist of 2 or more points
            if (RootArea != null && hasRouteInfo)
            {
                //replace start and end points with accurate ones
                var routePoints = routeInformation.ToWindows().ToList();
                routePoints.Clear();
                routePoints.Add(p1);
                routePoints.Add(p2);

                if (routedEdge.RoutingPoints != null)
                    routedEdge.RoutingPoints = routePoints.ToArray().ToGraphX();

                if (RootArea.EdgeCurvingEnabled)
                {
                    var oPolyLineSegment = GeometryHelper.GetCurveThroughPoints(routePoints.ToArray(), 0.5, RootArea.EdgeCurvingTolerance);

                    if (hasEpTarget)
                    {
                        UpdateTargetEpData(oPolyLineSegment.Points[oPolyLineSegment.Points.Count - 1], oPolyLineSegment.Points[oPolyLineSegment.Points.Count - 2]);
                        oPolyLineSegment.Points.RemoveAt(oPolyLineSegment.Points.Count - 1);
                    }
                    if (hasEpSource)
                    {
                        UpdateSourceEpData(oPolyLineSegment.Points.First(), oPolyLineSegment.Points[1]);
                        oPolyLineSegment.Points.RemoveAt(0);
                    }

                    lineFigure = GeometryHelper.GetPathFigureFromPathSegments(routePoints[0], true, true, oPolyLineSegment);
#if WPF
                    //freeze and create resulting geometry
                    GeometryHelper.TryFreeze(oPolyLineSegment);
#endif
                }
                else
                {
                    if (hasEpSource)
                        routePoints[0] = routePoints[0].Subtract(UpdateSourceEpData(routePoints.First(), routePoints[1]));
                    if (hasEpTarget)
                        routePoints[routePoints.Count - 1] = routePoints[routePoints.Count - 1].Subtract(UpdateTargetEpData(p2, routePoints[routePoints.Count - 2]));

                    // Reverse the path if specified.
                    if (gEdge.ReversePath)
                        routePoints.Reverse();

                    var pcol = new PointCollection();
                    routePoints.ForEach(a => pcol.Add(a));

                    lineFigure = new PathFigure { StartPoint = p1, Segments = new PathSegmentCollection { new PolyLineSegment { Points = pcol } }, IsClosed = false };
                }
            }
            else // no route defined
            {
                bool remainHidden = false;
                //check for hide only if prop is not 0
                if (HideEdgePointerByEdgeLength != 0d)
                {
                    if (MathHelper.GetDistanceBetweenPoints(p1, p2) <= HideEdgePointerByEdgeLength)
                    {
                        EdgePointerForSource?.Hide();
                        EdgePointerForTarget?.Hide();
                        remainHidden = true;
                    }
                    else
                    {
                        EdgePointerForSource?.Show();
                        EdgePointerForTarget?.Show();
                    }
                }

                if (hasEpSource)
                    p1 = p1.Subtract(UpdateSourceEpData(p1, p2, remainHidden));
                if (hasEpTarget)
                    p2 = p2.Subtract(UpdateTargetEpData(p2, p1, remainHidden));

                lineFigure = new PathFigure { StartPoint = gEdge.ReversePath ? p2 : p1, Segments = new PathSegmentCollection { new LineSegment() { Point = gEdge.ReversePath ? p1 : p2 } }, IsClosed = false };
            }
            ((PathGeometry)Linegeometry).Figures.Add(lineFigure);
#if WPF
            GeometryHelper.TryFreeze(lineFigure);
            GeometryHelper.TryFreeze(Linegeometry);
#endif
            if (ShowLabel && EdgeLabelControl != null && _updateLabelPosition)
                EdgeLabelControl.UpdatePosition();

            if (LinePathObject == null) return;
            LinePathObject.Data = Linegeometry;
            LinePathObject.StrokeDashArray = StrokeDashArray;
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
            if ((Visibility != Visibility.Visible && !IsHiddenEdgesUpdated) && this.Source == null || this.Target == null || ManualDrawing || !IsTemplateLoaded) return;

            #region Get the inputs

            //get the size of the source
            var sourceSize = new Size
            {
                Width = this.Source.ActualWidth,
                Height = this.Source.ActualHeight
            };
            if (CustomHelper.IsInDesignMode(this)) sourceSize = new Size(80, 20);

            //get the position center of the source
            var sourcePos = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(this.Source) : GraphAreaBase.GetFinalX(this.Source)) + sourceSize.Width * .5,
                Y = (useCurrentCoords ? GraphAreaBase.GetY(this.Source) : GraphAreaBase.GetFinalY(this.Source)) + sourceSize.Height * .5
            };

            //get the size of the target
            var targetSize = new Size
            {
                Width = this.Target.ActualWidth,
                Height = this.Target.ActualHeight
            };
            if (CustomHelper.IsInDesignMode(this))
                targetSize = new Size(80, 20);

            //get the position center of the target
            var targetPos = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(this.Target) : GraphAreaBase.GetFinalX(this.Target)) + targetSize.Width * .5,
                Y = (useCurrentCoords ? GraphAreaBase.GetY(this.Target) : GraphAreaBase.GetFinalY(this.Target)) + targetSize.Height * .5
            };

            var routedEdge = Edge as IRoutingInfo;
            if (routedEdge == null)
                throw new GX_InvalidDataException("Edge must implement IRoutingInfo interface");

            //get the route informations
            var routeInformation = externalRoutingPoints ?? routedEdge.RoutingPoints;

            // Get the TopLeft position of the Source Vertex.
            var sourcePos1 = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(this.Source) : GraphAreaBase.GetFinalX(this.Source)),
                Y = (useCurrentCoords ? GraphAreaBase.GetY(this.Source) : GraphAreaBase.GetFinalY(this.Source))
            };
            // Get the TopLeft position of the Target Vertex.
            var targetPos1 = new Point
            {
                X = (useCurrentCoords ? GraphAreaBase.GetX(this.Target) : GraphAreaBase.GetFinalX(this.Target)),
                Y = (useCurrentCoords ? GraphAreaBase.GetY(this.Target) : GraphAreaBase.GetFinalY(this.Target))
            };

            var hasEpSource = EdgePointerForSource != null;
            var hasEpTarget = EdgePointerForTarget != null;

            #endregion Get the inputs

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
                sourcePos = GetParallelOffset(this.Source, this.Target, ParallelEdgeOffset);
                targetPos = GetParallelOffset(this.Target, this.Source, -ParallelEdgeOffset);
            }

            /* Rectangular shapes implementation by bleibold */

            var gEdge = Edge as IGraphXCommonEdge;
            Point p1;
            Point p2;

            //calculate edge source (p1) and target (p2) endpoints based on different settings
            if (gEdge?.SourceConnectionPointId != null)
            {
                var sourceCp = this.Source.GetConnectionPointById(gEdge.SourceConnectionPointId.Value, true);
                if (sourceCp == null)
                    throw new GX_ObjectNotFoundException(string.Format("Can't find source vertex VCP by edge source connection point Id({1}) : {0}", this.Source, gEdge.SourceConnectionPointId));
                if (sourceCp.Shape == VertexShape.None) p1 = sourceCp.RectangularSize.Center();
                else
                {
                    var targetCpPos = gEdge.TargetConnectionPointId.HasValue ? this.Target.GetConnectionPointById(gEdge.TargetConnectionPointId.Value, true).RectangularSize.Center() : (hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos));
                    p1 = GeometryHelper.GetEdgeEndpoint(sourceCp.RectangularSize.Center(), sourceCp.RectangularSize, targetCpPos, sourceCp.Shape);
                }
            }
            else
                p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new SysRect(sourcePos1, sourceSize), (hasRouteInfo ? routeInformation[1].ToWindows() : (targetPos)), this.Source.VertexShape);

            if (gEdge?.TargetConnectionPointId != null)
            {
                var targetCp = this.Target.GetConnectionPointById(gEdge.TargetConnectionPointId.Value, true);
                if (targetCp == null)
                    throw new GX_ObjectNotFoundException(string.Format("Can't find target vertex VCP by edge target connection point Id({1}) : {0}", this.Target, gEdge.TargetConnectionPointId));
                if (targetCp.Shape == VertexShape.None) p2 = targetCp.RectangularSize.Center();
                else
                {
                    var sourceCpPos = gEdge.SourceConnectionPointId.HasValue ? this.Source.GetConnectionPointById(gEdge.SourceConnectionPointId.Value, true).RectangularSize.Center() : hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos);
                    p2 = GeometryHelper.GetEdgeEndpoint(targetCp.RectangularSize.Center(), targetCp.RectangularSize, sourceCpPos, targetCp.Shape);
                }
            }
            else
                p2 = GeometryHelper.GetEdgeEndpoint(targetPos, new SysRect(targetPos1, targetSize), hasRouteInfo ? routeInformation[routeInformation.Length - 2].ToWindows() : (sourcePos), this.Target.VertexShape);

            SourceConnectionPoint = p1;
            TargetConnectionPoint = p2;

            Linegeometry = new PathGeometry();
            PathFigure lineFigure;

            //if we have route and route consist of 2 or more points
            if (RootArea != null && hasRouteInfo)
            {
                //replace start and end points with accurate ones
                var routePoints = routeInformation.ToWindows().ToList();
                routePoints.Remove(routePoints.First());
                routePoints.Remove(routePoints.Last());
                routePoints.Insert(0, p1);
                routePoints.Add(p2);

                if (externalRoutingPoints == null && routedEdge.RoutingPoints != null)
                    routedEdge.RoutingPoints = routePoints.ToArray().ToGraphX();

                if (RootArea.EdgeCurvingEnabled)
                {
                    var oPolyLineSegment = GeometryHelper.GetCurveThroughPoints(routePoints.ToArray(), 0.5, RootArea.EdgeCurvingTolerance);

                    if (hasEpTarget)
                    {
                        UpdateTargetEpData(oPolyLineSegment.Points[oPolyLineSegment.Points.Count - 1], oPolyLineSegment.Points[oPolyLineSegment.Points.Count - 2]);
                        oPolyLineSegment.Points.RemoveAt(oPolyLineSegment.Points.Count - 1);
                    }
                    if (hasEpSource)
                    {
                        UpdateSourceEpData(oPolyLineSegment.Points.First(), oPolyLineSegment.Points[1]);
                        oPolyLineSegment.Points.RemoveAt(0);
                    }

                    lineFigure = GeometryHelper.GetPathFigureFromPathSegments(routePoints[0], true, true, oPolyLineSegment);
#if WPF
                    //freeze and create resulting geometry
                    GeometryHelper.TryFreeze(oPolyLineSegment);
#endif
                }
                else
                {
                    if (hasEpSource)
                        routePoints[0] = routePoints[0].Subtract(UpdateSourceEpData(routePoints.First(), routePoints[1]));
                    if (hasEpTarget)
                        routePoints[routePoints.Count - 1] = routePoints[routePoints.Count - 1].Subtract(UpdateTargetEpData(p2, routePoints[routePoints.Count - 2]));

                    // Reverse the path if specified.
                    if (gEdge.ReversePath)
                        routePoints.Reverse();

                    var pcol = new PointCollection();
                    routePoints.ForEach(a => pcol.Add(a));

                    lineFigure = new PathFigure { StartPoint = p1, Segments = new PathSegmentCollection { new PolyLineSegment { Points = pcol } }, IsClosed = false };
                }
            }
            else // no route defined
            {
                bool allowUpdateEpDataToUnsuppress = true;
                //check for hide only if prop is not 0
                if (HideEdgePointerByEdgeLength != 0d)
                {
                    if (MathHelper.GetDistanceBetweenPoints(p1, p2) <= HideEdgePointerByEdgeLength)
                    {
                        EdgePointerForSource?.Suppress();
                        EdgePointerForTarget?.Suppress();
                        allowUpdateEpDataToUnsuppress = false;
                    }
                    else
                    {
                        EdgePointerForSource?.UnSuppress();
                        EdgePointerForTarget?.UnSuppress();
                    }
                }

                if (hasEpSource)
                    p1 = p1.Subtract(UpdateSourceEpData(p1, p2, allowUpdateEpDataToUnsuppress));
                if (hasEpTarget)
                    p2 = p2.Subtract(UpdateTargetEpData(p2, p1, allowUpdateEpDataToUnsuppress));

                lineFigure = new PathFigure { StartPoint = gEdge.ReversePath ? p2 : p1, Segments = new PathSegmentCollection { new LineSegment() { Point = gEdge.ReversePath ? p1 : p2 } }, IsClosed = false };
            }
            ((PathGeometry)Linegeometry).Figures.Add(lineFigure);
#if WPF
            GeometryHelper.TryFreeze(lineFigure);
            GeometryHelper.TryFreeze(Linegeometry);
#endif
            if (ShowLabel && EdgeLabelControl != null && _updateLabelPosition && updateLabel)
                EdgeLabelControl.UpdatePosition();
        }

        private Point UpdateSourceEpData(Point from, Point to, bool allowUnsuppress = true)
        {
            var dir = MathHelper.GetDirection(from, to);
            if (from == to)
            {
                if (HideEdgePointerOnVertexOverlap) EdgePointerForSource.Suppress();
                else dir = new Vector(0, 0);
            }
            else if (allowUnsuppress) EdgePointerForSource.UnSuppress();
            var result = EdgePointerForSource.Update(from, dir, EdgePointerForSource.NeedRotation ? -MathHelper.GetAngleBetweenPoints(from, to).ToDegrees() : 0);
            return EdgePointerForSource.Visibility == Visibility.Visible ? result : new Point();
        }

        private Point UpdateTargetEpData(Point from, Point to, bool allowUnsuppress = true)
        {
            var dir = MathHelper.GetDirection(from, to);
            if (from == to)
            {
                if (HideEdgePointerOnVertexOverlap) EdgePointerForTarget.Suppress();
                else dir = new Vector(0, 0);
            }
            else if (allowUnsuppress) EdgePointerForTarget.UnSuppress();
            var result = EdgePointerForTarget.Update(from, dir, EdgePointerForTarget.NeedRotation ? (-MathHelper.GetAngleBetweenPoints(from, to).ToDegrees()) : 0);
            return EdgePointerForTarget.Visibility == Visibility.Visible ? result : new Point();
        }

        #endregion public PrepareEdgePath()

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
            return EdgeLabelControl.GetSize();
        }

        public void SetCustomLabelSize(SysRect rect)
        {
            EdgeLabelControl.SetSize(rect);
        }

        internal virtual void UpdateLabelLayout(bool force = false)
        {
            EdgeLabelControl.Show();
            if (EdgeLabelControl.GetSize() == SysRect.Empty || force)

            {
                EdgeLabelControl.UpdateLayout();
                EdgeLabelControl.UpdatePosition();
            }
        }
    }
}