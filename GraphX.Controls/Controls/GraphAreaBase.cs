using System;
using System.Collections.Generic;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using Windows.ApplicationModel;
#endif
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Interfaces;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;

namespace GraphX
{
    public abstract class GraphAreaBase : Canvas, ITrackableContent
    {
        /// <summary>
        /// Gets or Sets if GraphArea is in print mode when its size is recalculated on each Measure pass
        /// </summary>
        protected bool IsInPrintMode;

        public abstract void SetPrintMode(bool value, bool offsetControls = true, int margin = 0);

        /// <summary>
        /// Automaticaly assign unique Id (if missing) for vertex and edge data classes provided as Graph in GenerateGraph() method or by Addvertex() or AddEdge() methods
        /// </summary>
        public bool AutoAssignMissingDataId { get; set; }

        /// <summary>
        /// Action that will take place when LogicCore property is changed. Default: None.
        /// </summary>
        public LogicCoreChangedAction LogicCoreChangeAction
        {
            get { return (LogicCoreChangedAction)GetValue(LogicCoreChangeActionProperty); }
            set { SetValue(LogicCoreChangeActionProperty, value); }
        }

        public static readonly DependencyProperty LogicCoreChangeActionProperty =
            DependencyProperty.Register("LogicCoreChangeAction", typeof(LogicCoreChangedAction), typeof(GraphAreaBase), new PropertyMetadata(LogicCoreChangedAction.None));

#if WPF
        /// <summary>
        /// Gets or sets special mode for WinForms interoperability
        /// </summary>
        public bool EnableWinFormsHostingMode { get; set; }
#endif

        protected GraphAreaBase()
        {
            AutoAssignMissingDataId = true;
            LogicCoreChangeAction = LogicCoreChangedAction.None;
        }

        #region Attached Dependency Property registrations

        public static readonly DependencyProperty XProperty =
            DependencyProperty.RegisterAttached("X", typeof (double), typeof (GraphAreaBase),
#if WPF
                                                 new FrameworkPropertyMetadata(double.NaN,
                                                                                FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                FrameworkPropertyMetadataOptions.AffectsArrange |
                                                                                FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                                                                                FrameworkPropertyMetadataOptions.AffectsParentArrange |
                                                                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, x_changed));
#elif METRO
                                                 new PropertyMetadata(double.NaN, x_changed));
#endif

        private static void x_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(LeftProperty, e.NewValue);
        }

        public static readonly DependencyProperty FinalXProperty =
            DependencyProperty.RegisterAttached("FinalX", typeof(double), typeof(GraphAreaBase),
                                                 new PropertyMetadata(double.NaN));
        public static readonly DependencyProperty FinalYProperty =
            DependencyProperty.RegisterAttached("FinalY", typeof(double), typeof(GraphAreaBase),
                                                 new PropertyMetadata(double.NaN));



        public static readonly DependencyProperty YProperty =
            DependencyProperty.RegisterAttached("Y", typeof(double), typeof(GraphAreaBase),
#if WPF
                                                 new FrameworkPropertyMetadata(double.NaN,
                                                   FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                   FrameworkPropertyMetadataOptions.AffectsArrange |
                                                   FrameworkPropertyMetadataOptions.AffectsRender |
                                                   FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                                                   FrameworkPropertyMetadataOptions.AffectsParentArrange |
                                                   FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                                                   , y_changed));
#elif METRO
                                                 new PropertyMetadata(double.NaN, y_changed));
#endif

        private static void y_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(TopProperty, e.NewValue);
        }

        public static double GetX(DependencyObject obj)
        {
#if WPF
            return (double)obj.GetValue(XProperty);
#elif METRO
            return (double)obj.GetValue(LeftProperty);
#endif
        }

        public static void SetX(DependencyObject obj, double value, bool alsoSetFinal = true)
        {
            obj.SetValue(XProperty, value);
            if (alsoSetFinal)
                obj.SetValue(FinalXProperty, value);
        }

        public static double GetY(DependencyObject obj)
        {
#if WPF
            return (double)obj.GetValue(YProperty);
#elif METRO
            return (double)obj.GetValue(TopProperty);
#endif
        }

        public static void SetY(DependencyObject obj, double value, bool alsoSetFinal = false)
        {
            obj.SetValue(YProperty, value);
            if (alsoSetFinal)
                obj.SetValue(FinalYProperty, value);
        }


        public static double GetFinalX(DependencyObject obj)
        {
            return (double)obj.GetValue(FinalXProperty);
        }

        public static void SetFinalX(DependencyObject obj, double value)
        {
            obj.SetValue(FinalXProperty, value);
        }

        public static double GetFinalY(DependencyObject obj)
        {
            return (double)obj.GetValue(FinalYProperty);
        }

        public static void SetFinalY(DependencyObject obj, double value)
        {
            obj.SetValue(FinalYProperty, value);
        }

        public static bool GetPositioningComplete(DependencyObject obj)
        {
            return (bool)obj.GetValue(PositioningCompleteProperty);
        }

        public static void SetPositioningComplete(DependencyObject obj, bool value)
        {
            obj.SetValue(PositioningCompleteProperty, value);
        }

        #region DP - ExternalSettings
        public static readonly DependencyProperty ExternalSettingsProperty = DependencyProperty.Register("ExternalSettingsOnly", typeof(object),
                                        typeof(GraphAreaBase), new PropertyMetadata(null));
        /// <summary>
        ///User-defined settings storage for using in templates and converters
        /// </summary>
        public object ExternalSettings
        {
            get { return GetValue(ExternalSettingsProperty); }
            set { SetValue(ExternalSettingsProperty, value); }
        }
        #endregion

        #region DP - Animations

        /// <summary>
        /// Gets or sets vertex and edge controls animation
        /// </summary>
        public MoveAnimationBase MoveAnimation
        {
            get { return (MoveAnimationBase)GetValue(MoveAnimationProperty); }
            set { SetValue(MoveAnimationProperty, value); }
        }

        public static readonly DependencyProperty MoveAnimationProperty =
            DependencyProperty.Register("MoveAnimation", typeof(MoveAnimationBase), typeof(GraphAreaBase), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets vertex and edge controls delete animation
        /// </summary>
        public IOneWayControlAnimation DeleteAnimation
        {
            get { return (IOneWayControlAnimation)GetValue(DeleteAnimationProperty); }
            set { SetValue(DeleteAnimationProperty, value); }
        }

        public static readonly DependencyProperty DeleteAnimationProperty =
            DependencyProperty.Register("DeleteAnimation", typeof(IOneWayControlAnimation), typeof(GraphAreaBase), new PropertyMetadata(null, DeleteAnimationPropertyChanged));

        private static void DeleteAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var animation = e.OldValue as IOneWayControlAnimation;
            if (animation != null)
            {
                var old = animation;
                old.Completed -= GraphAreaBase_Completed;
            }
            var newone = e.NewValue as IOneWayControlAnimation;
            if (newone != null)
                newone.Completed += GraphAreaBase_Completed;

        }

        static void GraphAreaBase_Completed(object sender, ControlEventArgs e)
        {
            if (e.Control == null || e.Control.RootArea == null) return;
            e.Control.RootArea.RemoveAnimatedControl(e.Control, e.RemoveDataObject);
        }

        /// <summary>
        /// Deletes vertices and edges correctly after delete animation
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="removeDataObject">Also remove data object from data graph if possible</param>
        protected abstract void RemoveAnimatedControl(IGraphControl ctrl, bool removeDataObject);

        /// <summary>
        /// Gets or sets vertex and edge controls mouse over animation
        /// </summary>
        public IBidirectionalControlAnimation MouseOverAnimation
        {
            get { return (IBidirectionalControlAnimation)GetValue(MouseOverAnimationProperty); }
            set { SetValue(MouseOverAnimationProperty, value); }
        }

        public static readonly DependencyProperty MouseOverAnimationProperty =
            DependencyProperty.Register("MouseOverAnimation", typeof(IBidirectionalControlAnimation), typeof(GraphAreaBase), new PropertyMetadata(null));

        #endregion

        public static readonly DependencyProperty PositioningCompleteProperty =
            DependencyProperty.RegisterAttached("PositioningComplete", typeof(bool), typeof(GraphAreaBase), new PropertyMetadata(true));

        #endregion

        #region Child EVENTS

        internal static readonly Size DesignSize = new Size(70, 25);

        /// <summary>
        /// Fires when ContentSize property is changed
        /// </summary>
        public event ContentSizeChangedEventHandler ContentSizeChanged;

        protected void OnContentSizeChanged(Rect oldSize, Rect newSize)
        {
            if (ContentSizeChanged != null)
                ContentSizeChanged(this, new ContentSizeChangedEventArgs(oldSize, newSize));
        }

        /// <summary>
        /// Fires when edge is selected
        /// </summary>
        public event EdgeSelectedEventHandler EdgeSelected;

        internal virtual void OnEdgeSelected(EdgeControl ec, MouseButtonEventArgs e, ModifierKeys keys)
        {
            if (EdgeSelected != null)
                EdgeSelected(this, new EdgeSelectedEventArgs(ec, e
#if WPF
                    , keys));
#elif METRO
                    ));
#endif
        }

        /// <summary>
        /// Fires when vertex is double clicked
        /// </summary>
        public event VertexSelectedEventHandler VertexDoubleClick;

#if WPF
        internal virtual void OnVertexDoubleClick(VertexControl vc, MouseButtonEventArgs e)
        {
            if (VertexDoubleClick != null)
                VertexDoubleClick(this, new VertexSelectedEventArgs(vc, e, Keyboard.Modifiers));
        }
#elif METRO
        internal virtual void OnVertexDoubleClick(VertexControl vc, PointerRoutedEventArgs e)
        {
            if (VertexDoubleClick != null)
                VertexDoubleClick(this, new VertexSelectedEventArgs(vc, e));
        }
#endif

        /// <summary>
        /// Fires when vertex is selected
        /// </summary>
        public event VertexSelectedEventHandler VertexSelected;

        internal virtual void OnVertexSelected(VertexControl vc, MouseButtonEventArgs e, ModifierKeys keys)
        {
            if (VertexSelected != null)
                VertexSelected(this, new VertexSelectedEventArgs(vc, e
#if WPF
                    , keys));
#elif METRO
                    ));
#endif
        }

        /// <summary>
        /// Fires when mouse up on vertex
        /// </summary>
        public event VertexSelectedEventHandler VertexMouseUp;

        internal virtual void OnVertexMouseUp(VertexControl vc, MouseButtonEventArgs e, ModifierKeys keys)
        {
            if (VertexMouseUp != null)
                VertexMouseUp(this, new VertexSelectedEventArgs(vc, e
#if WPF
                    , keys));
#elif METRO
                    ));
#endif
        }
        /// <summary>
        /// Fires when mouse is over the vertex control
        /// </summary>
        public event VertexSelectedEventHandler VertexMouseEnter;

        internal virtual void OnVertexMouseEnter(VertexControl vc, MouseEventArgs e)
        {
            if (VertexMouseEnter != null)
                VertexMouseEnter(this, new VertexSelectedEventArgs(vc, e
#if WPF
                    , Keyboard.Modifiers));
#elif METRO
                    ));
#endif
            if (MouseOverAnimation != null)
                MouseOverAnimation.AnimateVertexForward(vc);
        }

        /// <summary>
        /// Fires when mouse is moved over the vertex control
        /// </summary>
        public event VertexMovedEventHandler VertexMouseMove;

        internal virtual void OnVertexMouseMove(VertexControl vc, MouseEventArgs e)
        {
            if (VertexMouseMove != null)
                VertexMouseMove(this, new VertexMovedEventArgs(vc, e));
        }

        /// <summary>
        /// Fires when mouse leaves vertex control
        /// </summary>
        public event VertexSelectedEventHandler VertexMouseLeave;

        internal virtual void OnVertexMouseLeave(VertexControl vc, MouseEventArgs e)
        {
            if (VertexMouseLeave != null)
                VertexMouseLeave(this, new VertexSelectedEventArgs(vc, e
#if WPF
                    , Keyboard.Modifiers));
#elif METRO
));
#endif
            if (MouseOverAnimation != null)
                MouseOverAnimation.AnimateVertexBackward(vc);
        }

        /// <summary>
        /// Fires when layout algorithm calculation is finished
        /// </summary>
        public event EventHandler LayoutCalculationFinished;

        protected virtual void OnLayoutCalculationFinished()
        {
            if (LayoutCalculationFinished != null)
                LayoutCalculationFinished(this, null);
        }

        /// <summary>
        /// Fires when overlap removal algorithm calculation is finished
        /// </summary>
        public event EventHandler OverlapRemovalCalculationFinished;

        protected virtual void OnOverlapRemovalCalculationFinished()
        {
            if (OverlapRemovalCalculationFinished != null)
                OverlapRemovalCalculationFinished(this, null);
        }

        /// <summary>
        /// Fires when edge routing algorithm calculation is finished
        /// </summary>
        public event EventHandler EdgeRoutingCalculationFinished;

        protected virtual void OnEdgeRoutingCalculationFinished()
        {
            if (EdgeRoutingCalculationFinished != null)
                EdgeRoutingCalculationFinished(this, null);
        }

        /// <summary>
        /// Fires when relayout operation is finished
        /// </summary>
        public event EventHandler RelayoutFinished;

        protected virtual void OnRelayoutFinished()
        {
            if (RelayoutFinished != null)
                RelayoutFinished(this, null);
        }

        /// <summary>
        /// Fires when graph generation operation is finished
        /// </summary>
        public event EventHandler GenerateGraphFinished;

        protected virtual void OnGenerateGraphFinished()
        {
            if (GenerateGraphFinished != null)
                GenerateGraphFinished(this, null);
        }

        public event EdgeSelectedEventHandler EdgeDoubleClick;
        internal void OnEdgeDoubleClick(EdgeControl edgeControl, MouseButtonEventArgs e, ModifierKeys keys)
        {
            if (EdgeDoubleClick != null)
                EdgeDoubleClick(this, new EdgeSelectedEventArgs(edgeControl, e
#if WPF
                    , keys));
#elif METRO
                    ));
#endif                    
        }

        public event EdgeSelectedEventHandler EdgeMouseMove;
        internal void OnEdgeMouseMove(EdgeControl edgeControl, MouseButtonEventArgs e, ModifierKeys keys)
        {
            if (EdgeMouseMove != null)
                EdgeMouseMove(this, new EdgeSelectedEventArgs(edgeControl, e
#if WPF
                    , keys));
#elif METRO
));
#endif
        }

        public event EdgeSelectedEventHandler EdgeMouseEnter;
        internal void OnEdgeMouseEnter(EdgeControl edgeControl, MouseButtonEventArgs e, ModifierKeys keys)
        {
            if (EdgeMouseEnter != null)
                EdgeMouseEnter(this, new EdgeSelectedEventArgs(edgeControl, e
#if WPF
                    , keys));
#elif METRO
));
#endif
            if (MouseOverAnimation != null)
                MouseOverAnimation.AnimateEdgeForward(edgeControl);
        }

        public event EdgeSelectedEventHandler EdgeMouseLeave;
        internal void OnEdgeMouseLeave(EdgeControl edgeControl, MouseButtonEventArgs e, ModifierKeys keys)
        {
            if (EdgeMouseLeave != null)
                EdgeMouseLeave(this, new EdgeSelectedEventArgs(edgeControl, e
#if WPF
                    , keys));
#elif METRO
));
#endif
            if (MouseOverAnimation != null)
                MouseOverAnimation.AnimateEdgeBackward(edgeControl);
        }

        #endregion

        #region ComputeEdgeRoutesByVertex()
        /// <summary>
        /// Compute new edge routes for all edges of the vertex
        /// </summary>
        /// <param name="vc">Vertex visual control</param>
        /// <param name="vertexDataNeedUpdate">If vertex data inside edge routing algorthm needs to be updated</param>
        internal virtual void ComputeEdgeRoutesByVertex(VertexControl vc, bool vertexDataNeedUpdate = true) { }
        #endregion

        #region Virtual members
        /// <summary>
        /// Returns all existing VertexControls addded into the layout
        /// </summary>
        /// <returns></returns>
        public abstract VertexControl[] GetAllVertexControls();

        // INTERNAL VARIABLES FOR CONTROLS INTEROPERABILITY
        internal abstract bool IsEdgeRoutingEnabled { get; }
        internal abstract bool EnableParallelEdges { get; }
        internal abstract bool EdgeCurvingEnabled { get; }
        internal abstract double EdgeCurvingTolerance { get; }


        /// <summary>
        /// Get controls related to specified control 
        /// </summary>
        /// <param name="ctrl">Original control</param>
        /// <param name="resultType">Type of resulting related controls</param>
        /// <param name="edgesType">Optional edge controls type</param>
        public abstract List<IGraphControl> GetRelatedControls(IGraphControl ctrl, GraphControlType resultType = GraphControlType.VertexAndEdge, EdgesType edgesType = EdgesType.Out);
        /// <summary>
        /// Get vertex controls related to specified control 
        /// </summary>
        /// <param name="ctrl">Original control</param>
        /// <param name="edgesType">Edge types to query for vertices</param>
        public abstract List<IGraphControl> GetRelatedVertexControls(IGraphControl ctrl, EdgesType edgesType = EdgesType.All);
        /// <summary>
        /// Get edge controls related to specified control 
        /// </summary>
        /// <param name="ctrl">Original control</param>
        /// <param name="edgesType">Edge types to query</param>
        public abstract List<IGraphControl> GetRelatedEdgeControls(IGraphControl ctrl, EdgesType edgesType = EdgesType.All);


        /// <summary>
        /// Generates and displays edges for specified vertex
        /// </summary>
        /// <param name="vc">Vertex control</param>
        /// <param name="edgeType">Type of edges to display</param>
        /// <param name="defaultVisibility">Default edge visibility on layout</param>
        public abstract void GenerateEdgesForVertex(VertexControl vc, EdgesType edgeType, Visibility defaultVisibility = Visibility.Visible);

        #endregion


        #region Measure & Arrange

        /// <summary>
        /// The position of the topLeft corner of the most top-left or top left object if UseNativeObjectArrange == false
        /// vertex.
        /// </summary>
        private Point _topLeft;

        /// <summary>
        /// The position of the bottom right corner of the most or bottom right object if UseNativeObjectArrange == false
        /// bottom-right vertex.
        /// </summary>
        private Point _bottomRight;

        /// <summary>
        /// Gets the size of the GraphArea taking into account positions of the children
        /// This is the main size pointer. Don't use DesiredSize or ActualWidth props as they are simulated.
        /// </summary>
        public Rect ContentSize { get { return new Rect(_topLeft, _bottomRight); } }

        /// <summary>
        /// Translation of the GraphArea object
        /// </summary>
// public Vector Translation { get; private set; }
        /// <summary>
        /// Gets or sets additional area space for each side of GraphArea. Useful for zoom adjustments.
        /// 0 by default.
        /// </summary>
        public Size SideExpansionSize { get; set; }
        /// <summary>
        /// Gets or sets if edge route paths must be taken into consideration while determining area size
        /// </summary>
        private const bool COUNT_ROUTE_PATHS = true;

        /// <summary>
        /// Arranges the size of the control.
        /// </summary>
        /// <param name="arrangeSize">The arranged size of the control.</param>
        /// <returns>The size of the control.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {

            var minPoint = new Point(double.PositiveInfinity, double.PositiveInfinity);
            var maxPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
#if WPF
            foreach (UIElement child in InternalChildren)
#elif METRO
            foreach (UIElement child in Children)
#endif            
            {
                var x = GetX(child);
                var y = GetY(child);

                if (double.IsNaN(x) || double.IsNaN(y))
                {
                    var ec = child as EdgeControl;
                    //not a vertex, set the coordinates of the top-left corner
                    if (ec != null)
                    {
                        x = 0;
                        y = 0;
                    }
#if METRO
                    else continue;
#endif
                    if (COUNT_ROUTE_PATHS && ec != null)
                    {
                        var routingInfo = ec.Edge as IRoutingInfo;
                        if (routingInfo != null) {
                            var rps = routingInfo.RoutingPoints;
                            if (rps != null)
                            {
                                foreach (var item in rps)
                                {
                                    minPoint = new Point(Math.Min(minPoint.X, item.X), Math.Min(minPoint.Y, item.Y));
                                    maxPoint = new Point(Math.Max(maxPoint.X, item.X), Math.Max(maxPoint.Y, item.Y));
                                }
                            }
                        }
                    }
                }
                else
                {
                    //get the top-left corner
                    //x -= child.DesiredSize.Width * 0.5;
                    //y -= child.DesiredSize.Height * 0.5;
                    minPoint = new Point(Math.Min(minPoint.X, x), Math.Min(minPoint.Y, y));
                    maxPoint = new Point(Math.Max(maxPoint.X, x), Math.Max(maxPoint.Y, y));
                }

                child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
            }
#if WPF

            return DesignerProperties.GetIsInDesignMode(this) ? DesignSize : (IsInPrintMode ? ContentSize.Size : new Size(10, 10));
#elif METRO
            return DesignMode.DesignModeEnabled ? DesignSize : new Size(10, 10);
#endif    
        }

        /// <summary>
        /// Overridden measure. It calculates a size where all of 
        /// of the vertices are visible.
        /// </summary>
        /// <param name="constraint">The size constraint.</param>
        /// <returns>The calculated size.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            var oldSize = ContentSize;
            _topLeft = new Point(double.PositiveInfinity, double.PositiveInfinity);
            _bottomRight = new Point(double.NegativeInfinity, double.NegativeInfinity);

#if WPF
            foreach (UIElement child in InternalChildren)
#elif METRO
            foreach (UIElement child in Children)
#endif    
            {
                //measure the child
                child.Measure(constraint);

                //get the position of the vertex
                var left = GetFinalX(child);
                var top = GetFinalY(child);

                if(child.Visibility == Visibility.Collapsed) continue;

                if (double.IsNaN(left) || double.IsNaN(top))
                {
                    var ec = child as EdgeControl;
                    if (!COUNT_ROUTE_PATHS || ec == null) continue;
                    var routingInfo = ec.Edge as IRoutingInfo;
                    if (routingInfo == null) continue;
                    var rps = routingInfo.RoutingPoints;
                    if (rps == null) continue;
                    foreach (var item in rps)
                    {
                        //get the top left corner point
                        _topLeft.X = Math.Min(_topLeft.X, item.X);
                        _topLeft.Y = Math.Min(_topLeft.Y, item.Y);

                        //calculate the bottom right corner point
                        _bottomRight.X = Math.Max(_bottomRight.X, item.X);
                        _bottomRight.Y = Math.Max(_bottomRight.Y, item.Y);
                    }
                }
                else
                {

                    //get the top left corner point
                    _topLeft.X = Math.Min(_topLeft.X, left);
                    _topLeft.Y = Math.Min(_topLeft.Y, top);

                    //calculate the bottom right corner point
                    _bottomRight.X = Math.Max(_bottomRight.X, left + child.DesiredSize.Width);
                    _bottomRight.Y = Math.Max(_bottomRight.Y, top + child.DesiredSize.Height);
                }

            }
            _topLeft.X -= SideExpansionSize.Width * .5;
            _topLeft.Y -= SideExpansionSize.Height * .5;
            _bottomRight.X += SideExpansionSize.Width * .5;
            _bottomRight.Y += SideExpansionSize.Height * .5;
            var newSize = ContentSize;
            if (oldSize != newSize)
                OnContentSizeChanged(oldSize, newSize);
#if WPF
            return DesignerProperties.GetIsInDesignMode(this) ? DesignSize : (IsInPrintMode ? ContentSize.Size : new Size(10, 10));
#elif METRO
            return DesignMode.DesignModeEnabled ? DesignSize : new Size(10, 10);
#endif
        }
        #endregion

    }
}
