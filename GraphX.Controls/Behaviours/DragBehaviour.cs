using System;
using System.Diagnostics;

#if WPF

using System.Windows;

#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using System.Linq;
#endif

using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Common.Models;

namespace GraphX.Controls
{
    /// <summary>
    /// Dragging behavior of objects in a GraphX graph area is influenced using the attached properties of this class.
    ///
    /// To enable dragging of an individual object, set the IsDragEnabled attached property to true on that object. When IsDragEnabled is true, the
    /// object can be used to initiate dragging.
    ///
    /// To drag a group of vertices, set the IsTagged attached property to true for all of the vertices in the group. When dragging is started from
    /// one of the tagged vertices, all of the tagged ones will be move.
    ///
    /// "Primary drag object" defined: Whichever object gets the mouse/pointer events is treated as the primary drag object and its attached properties take
    /// precedence for controlling drag behavior. When only one object is being dragged, it is the primary drag object. When a group of objects is tagged
    /// and being dragged together, the one getting mouse events is the primary drag object.
    ///
    /// There is limited support for dragging edges. It is achieved by setting IsDragEnabled to true for the edge AND tagging the edge and the vertices
    /// it is attached to. When the user drags the edge, the drag is actually performed on the vertices.
    ///
    /// For edges to be updated as a vertex is moved, set UpdateEdgesOnMove to true for the object being dragged.
    ///
    /// Snapping is turned on or off by the GlobalIsSnappingPredicate or by the IsSnappingPredicate property on the primary drag object. The predicate is
    /// called with each movement of the mouse/pointer and the primary drag object is passed in. If snapping should be performed, the predicate must return
    /// true. To skip snapping logic, the predicate must return false. If no predicate is set using the IsSnappingPredicate, the  GlobalIsSnappingPredicate
    /// is used. The default behavior is to snap while a shift key alone is pressed.
    ///
    /// When dragging a group of objects and using snapping, there is an additional refinement that can be used for the snapping behavior of the individual
    /// objects in the group. The individual objects can move the exact same amount as the primary object when it snaps, or they can snap individually, with
    /// the snap calculation being performed for each one. The behavior is controlled for the entire group by the GlobalIsSnappingIndividuallyPredicate or
    /// the IsIndividualSnappingPredicate property setting ON THE PRIMARY DRAG OBJECT. The default behavior is to move all dragged objects by the same offset
    /// as the primary drag object.
    ///
    /// Snapping calculations are performed by the functions set on the primary drag object using the GlobalXSnapModifier or XSnapModifier property and the
    /// GlobalYSnapModifier or YSnapModifier propery. These functions are called for each movement and provided the GraphAreaBase, object being moved, and
    /// the pre-snapped x or y value. The passed in parameters are intended to provide an opportunity to find elements within the graph area and do things
    /// like snap to center aligned, snap to left aligned, etc. The default behavior is to simply round the value to the nearest 10.
    /// </summary>
    public static class DragBehaviour
    {
        public delegate double SnapModifierFunc(GraphAreaBase area, DependencyObject obj, double val);

        #region Built-in snapping behavior

        private static readonly Predicate<DependencyObject> _builtinIsSnappingPredicate = obj =>
        {
#if WPF
            return System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Shift;
#elif METRO
            return Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift) == Windows.UI.Core.CoreVirtualKeyStates.Down;
#endif
        };

        private static readonly Predicate<DependencyObject> _builtinIsIndividualSnappingPredicate = obj => false;

        private static readonly SnapModifierFunc _builtinSnapModifier = (area, obj, val) => Math.Round(val * 0.1) * 10.0;

        #endregion Built-in snapping behavior

        #region Global snapping behavior management

        private static Predicate<DependencyObject> _globalIsSnappingPredicate = _builtinIsSnappingPredicate;

        /// <summary>
        /// Gets or sets the predicate used to determine whether to snap an object. The global predicate is used whenever the
        /// primary dragged object does not have a different predicate set using the IsSnappingPredicate attached property.
        /// </summary>
        /// <remarks>
        /// Setting to null will restore the built in behavior, but it is recommended to track the preceding value and restore that.
        /// </remarks>
        public static Predicate<DependencyObject> GlobalIsSnappingPredicate
        {
            get { return _globalIsSnappingPredicate; }
            set
            {
                if (value == null)
                {
                    _globalIsSnappingPredicate = _builtinIsSnappingPredicate;
                }
                else
                {
                    _globalIsSnappingPredicate = value;
                }
            }
        }

        private static Predicate<DependencyObject> _globalIsIndividualSnappingPredicate = _builtinIsIndividualSnappingPredicate;

        /// <summary>
        /// Gets or sets the predicate used to determine whether to perform individual snapping on a group of dragged objects.
        /// The global predicate is used whenever the primary dragged object does not have a different predicate set using the
        /// IsIndividualSnappingPredicate attached property.
        /// </summary>
        /// <remarks>
        /// Setting to null will restore the built in behavior, but it is recommended to track the preceding value and restore that.
        /// </remarks>
        public static Predicate<DependencyObject> GlobalIsIndividualSnappingPredicate
        {
            get { return _globalIsIndividualSnappingPredicate; }
            set
            {
                if (value == null)
                {
                    _globalIsIndividualSnappingPredicate = _builtinIsIndividualSnappingPredicate;
                }
                else
                {
                    _globalIsIndividualSnappingPredicate = value;
                }
            }
        }

        private static SnapModifierFunc _globalXSnapModifier = _builtinSnapModifier;

        /// <summary>
        /// Gets or sets the X value modifier to use when snapping an object. The global modifier is used whenever the
        /// primary dragged object does not have a different modifier set using the XSnapModifier attached property.
        /// </summary>
        /// <remarks>
        /// Setting to null will restore the built in behavior, but it is recommended to track the preceding value and restore that.
        /// </remarks>
        public static SnapModifierFunc GlobalXSnapModifier
        {
            get { return _globalXSnapModifier; }
            set
            {
                if (value == null)
                {
                    _globalXSnapModifier = _builtinSnapModifier;
                }
                else
                {
                    _globalXSnapModifier = value;
                }
            }
        }

        private static SnapModifierFunc _globalYSnapModifier = _builtinSnapModifier;

        /// <summary>
        /// Gets or sets the Y value modifier to use when snapping an object. The global modifier is used whenever the
        /// primary dragged object does not have a different modifier set using the YSnapModifier attached property.
        /// </summary>
        /// <remarks>
        /// Setting to null will restore the built in behavior, but it is recommended to track the preceding value and restore that.
        /// </remarks>
        public static SnapModifierFunc GlobalYSnapModifier
        {
            get { return _globalYSnapModifier; }
            set
            {
                if (value == null)
                {
                    _globalYSnapModifier = _builtinSnapModifier;
                }
                else
                {
                    _globalYSnapModifier = value;
                }
            }
        }

        #endregion Global snapping behavior management

        #region Attached DPs

        public static readonly DependencyProperty IsDragEnabledProperty = DependencyProperty.RegisterAttached("IsDragEnabled", typeof(bool), typeof(DragBehaviour), new PropertyMetadata(false, OnIsDragEnabledPropertyChanged));
        public static readonly DependencyProperty UpdateEdgesOnMoveProperty = DependencyProperty.RegisterAttached("UpdateEdgesOnMove", typeof(bool), typeof(DragBehaviour), new PropertyMetadata(false));
        public static readonly DependencyProperty IsTaggedProperty = DependencyProperty.RegisterAttached("IsTagged", typeof(bool), typeof(DragBehaviour), new PropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(DragBehaviour), new PropertyMetadata(false));
        public static readonly DependencyProperty IsSnappingPredicateProperty = DependencyProperty.RegisterAttached("IsSnappingPredicate", typeof(Predicate<DependencyObject>), typeof(DragBehaviour), new PropertyMetadata(new Predicate<DependencyObject>(obj => { return _globalIsSnappingPredicate(obj); })));
        public static readonly DependencyProperty IsIndividualSnappingPredicateProperty = DependencyProperty.RegisterAttached("IsIndividualSnappingPredicate", typeof(Predicate<DependencyObject>), typeof(DragBehaviour), new PropertyMetadata(new Predicate<DependencyObject>(obj => { return _globalIsIndividualSnappingPredicate(obj); })));

        /// <summary>
        /// Snap feature modifier delegate for X axis
        /// </summary>
        public static readonly DependencyProperty XSnapModifierProperty = DependencyProperty.RegisterAttached("XSnapModifier", typeof(SnapModifierFunc), typeof(DragBehaviour), new PropertyMetadata(new SnapModifierFunc((area, obj, val) => _globalXSnapModifier(area, obj, val))));

        /// <summary>
        /// Snap feature modifier delegate for Y axis
        /// </summary>
        public static readonly DependencyProperty YSnapModifierProperty = DependencyProperty.RegisterAttached("YSnapModifier", typeof(SnapModifierFunc), typeof(DragBehaviour), new PropertyMetadata(new SnapModifierFunc((area, obj, val) => _globalYSnapModifier(area, obj, val))));

        private static readonly DependencyProperty OriginalXProperty = DependencyProperty.RegisterAttached("OriginalX", typeof(double), typeof(DragBehaviour), new PropertyMetadata(0.0));
        private static readonly DependencyProperty OriginalYProperty = DependencyProperty.RegisterAttached("OriginalY", typeof(double), typeof(DragBehaviour), new PropertyMetadata(0.0));
        private static readonly DependencyProperty OriginalMouseXProperty = DependencyProperty.RegisterAttached("OriginalMouseX", typeof(double), typeof(DragBehaviour), new PropertyMetadata(0.0));
        private static readonly DependencyProperty OriginalMouseYProperty = DependencyProperty.RegisterAttached("OriginalMouseY", typeof(double), typeof(DragBehaviour), new PropertyMetadata(0.0));

        #endregion Attached DPs

        #region Get/Set method for Attached Properties

        public static bool GetUpdateEdgesOnMove(DependencyObject obj)
        {
            return (bool)obj.GetValue(UpdateEdgesOnMoveProperty);
        }

        public static void SetUpdateEdgesOnMove(DependencyObject obj, bool value)
        {
            obj.SetValue(UpdateEdgesOnMoveProperty, value);
        }

        public static bool GetIsTagged(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsTaggedProperty);
        }

        public static void SetIsTagged(DependencyObject obj, bool value)
        {
            obj.SetValue(IsTaggedProperty, value);
        }

        public static bool GetIsDragEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragEnabledProperty);
        }

        public static void SetIsDragEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragEnabledProperty, value);
        }

        public static bool GetIsDragging(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDraggingProperty);
        }

        public static void SetIsDragging(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDraggingProperty, value);
        }

        public static Predicate<DependencyObject> GetIsSnappingPredicate(DependencyObject obj)
        {
            return (Predicate<DependencyObject>)obj.GetValue(IsSnappingPredicateProperty);
        }

        public static void SetIsSnappingPredicate(DependencyObject obj, Predicate<DependencyObject> value)
        {
            obj.SetValue(IsSnappingPredicateProperty, value);
        }

        public static Predicate<DependencyObject> GetIsIndividualSnappingPredicate(DependencyObject obj)
        {
            return (Predicate<DependencyObject>)obj.GetValue(IsIndividualSnappingPredicateProperty);
        }

        public static void SetIsIndividualSnappingPredicate(DependencyObject obj, Predicate<DependencyObject> value)
        {
            obj.SetValue(IsIndividualSnappingPredicateProperty, value);
        }

        public static SnapModifierFunc GetXSnapModifier(DependencyObject obj)
        {
            return (SnapModifierFunc)obj.GetValue(XSnapModifierProperty);
        }

        public static void SetXSnapModifier(DependencyObject obj, SnapModifierFunc value)
        {
            obj.SetValue(XSnapModifierProperty, value);
        }

        public static SnapModifierFunc GetYSnapModifier(DependencyObject obj)
        {
            return (SnapModifierFunc)obj.GetValue(YSnapModifierProperty);
        }

        public static void SetYSnapModifier(DependencyObject obj, SnapModifierFunc value)
        {
            obj.SetValue(YSnapModifierProperty, value);
        }

        #endregion Get/Set method for Attached Properties

        #region Get/Set methods for private Attached Properties

        private static double GetOriginalX(DependencyObject obj)
        {
            return (double)obj.GetValue(OriginalXProperty);
        }

        private static void SetOriginalX(DependencyObject obj, double value)
        {
            obj.SetValue(OriginalXProperty, value);
        }

        private static double GetOriginalY(DependencyObject obj)
        {
            return (double)obj.GetValue(OriginalYProperty);
        }

        private static void SetOriginalY(DependencyObject obj, double value)
        {
            obj.SetValue(OriginalYProperty, value);
        }

        private static double GetOriginalMouseX(DependencyObject obj)
        {
            return (double)obj.GetValue(OriginalMouseXProperty);
        }

        private static void SetOriginalMouseX(DependencyObject obj, double value)
        {
            obj.SetValue(OriginalMouseXProperty, value);
        }

        private static double GetOriginalMouseY(DependencyObject obj)
        {
            return (double)obj.GetValue(OriginalMouseYProperty);
        }

        private static void SetOriginalMouseY(DependencyObject obj, double value)
        {
            obj.SetValue(OriginalMouseYProperty, value);
        }

        #endregion Get/Set methods for private Attached Properties

        #region PropertyChanged callbacks

        private static void OnIsDragEnabledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
#if WPF
            var element = obj as IInputElement;
#elif METRO
            var element = obj as FrameworkElement;
#else
            throw new NotImplementedException();
#endif
            if (element == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
            {
                //register the event handlers
#if WPF
                if (element is VertexControl)
                {
                    element.MouseLeftButtonDown += OnVertexDragStarted;
                    element.PreviewMouseLeftButtonUp += OnVertexDragFinished;
                }
                else if (element is EdgeControl)
                {
                    element.MouseLeftButtonDown += OnEdgeDrageStarted;
                    element.PreviewMouseLeftButtonUp += OnEdgeDragFinished;
                }
#elif METRO
                element.PointerPressed += OnDragStarted;
                element.PointerReleased += OnDragFinished;
#endif
            }
            else
            {
                //unregister the event handlers
#if WPF
                if (element is VertexControl)
                {
                    element.MouseLeftButtonDown -= OnVertexDragStarted;
                    element.PreviewMouseLeftButtonUp -= OnVertexDragFinished;
                }
                else if (element is EdgeControl)
                {
                    element.MouseLeftButtonDown -= OnEdgeDrageStarted;
                    element.PreviewMouseLeftButtonUp -= OnEdgeDragFinished;
                }
#elif METRO
                element.PointerPressed -= OnDragStarted;
                element.PointerReleased -= OnDragFinished;
#endif
            }
        }

        #endregion PropertyChanged callbacks
#if WPF
        private static void OnEdgeDrageStarted(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject obj = sender as DependencyObject;

            SetIsDragging(obj, true);

            var element = obj as IInputElement;
            if (element != null)
            {
                element.CaptureMouse();
                element.MouseMove -= OnEdgeDragging;
                element.MouseMove += OnEdgeDragging;
            }
            else throw new GX_InvalidDataException("The control must be a descendent of the FrameworkElement or FrameworkContentElement!");

            e.Handled = false;
        }

        private static void OnEdgeDragging(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var obj = sender as DependencyObject;
            if (!GetIsDragging(obj))
                return;

            EdgeControl edgeControl = sender as EdgeControl;

            edgeControl.PrepareEdgePathFromMousePointer();

            e.Handled = true;
        }

        private static void OnEdgeDragFinished(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EdgeControl edgeControl = sender as EdgeControl;

            if (edgeControl == null) return;

            GraphAreaBase graphAreaBase = edgeControl.RootArea;

            VertexControl vertexControl = graphAreaBase.GetVertexControlAt(e.GetPosition(graphAreaBase));

            if (vertexControl != null)
            {
                edgeControl.Target = vertexControl;

                if (vertexControl.VertexConnectionPointsList.Count > 0)
                {
                    IVertexConnectionPoint vertexConnectionPoint = vertexControl.GetConnectionPointAt(e.GetPosition(graphAreaBase));

                    var edge = edgeControl.Edge as IGraphXCommonEdge;

                    if (vertexConnectionPoint != null)
                    {
                        edge.TargetConnectionPointId = vertexConnectionPoint.Id;
                    }
                    else
                    {
                        edge.TargetConnectionPointId = null;
                    }
                }

                edgeControl.UpdateEdge();

                var obj = (DependencyObject)sender;
                SetIsDragging(obj, false);
                //obj.ClearValue(OriginalMouseXProperty);
                //obj.ClearValue(OriginalMouseYProperty);
                //obj.ClearValue(OriginalXProperty);
                //obj.ClearValue(OriginalYProperty);

                var element = sender as IInputElement;
                if (element != null)
                {
                    element.MouseMove -= OnVertexDragging;
                    element.ReleaseMouseCapture();
                }
            }
        }
#endif
#if WPF

        private static void OnVertexDragStarted(object sender, System.Windows.Input.MouseButtonEventArgs e)
#elif METRO
        private static void OnDragStarted( object sender, PointerRoutedEventArgs e )
#endif
        {
            var obj = sender as DependencyObject;
            //we are starting the drag
            SetIsDragging(obj, true);

            // Save the position of the mouse to the start position
            var area = GetAreaFromObject(obj);
            var pos = GetPositionInArea(area, e);
            SetOriginalMouseX(obj, pos.X);
            SetOriginalMouseY(obj, pos.Y);

            // Save the position of the dragged object to its starting position
            SetOriginalX(obj, GraphAreaBase.GetFinalX(obj));
            SetOriginalY(obj, GraphAreaBase.GetFinalY(obj));

            // Save starting position of all other tagged elements
            foreach (var item in area.GetAllVertexControls())
                if (!ReferenceEquals(item, obj) && GetIsTagged(item))
                {
                    SetOriginalX(item, GraphAreaBase.GetFinalX(item));
                    SetOriginalY(item, GraphAreaBase.GetFinalY(item));
                }

            //capture the mouse
#if WPF
            var element = obj as IInputElement;
            if (element != null)
            {
                element.CaptureMouse();
                element.MouseMove -= OnVertexDragging;
                element.MouseMove += OnVertexDragging;
            }
            //else throw new GX_InvalidDataException("The control must be a descendent of the FrameworkElement or FrameworkContentElement!");
            e.Handled = false;
#elif METRO
            var element = obj as FrameworkElement;
            if ( element != null )
            {
                element.CapturePointer(e.Pointer);
                element.PointerMoved += OnDragging;
            }
            e.Handled = true;
#endif
        }

#if WPF

        private static void OnVertexDragFinished(object sender, System.Windows.Input.MouseButtonEventArgs e)
#elif METRO
        private static void OnDragFinished( object sender, PointerRoutedEventArgs e )
#endif
        {
            UpdateVertexEdges(sender as VertexControl);

            var obj = (DependencyObject)sender;
            SetIsDragging(obj, false);
            obj.ClearValue(OriginalMouseXProperty);
            obj.ClearValue(OriginalMouseYProperty);
            obj.ClearValue(OriginalXProperty);
            obj.ClearValue(OriginalYProperty);
            if (GetIsTagged(obj))
            {
                var area = GetAreaFromObject(obj);
                foreach (var item in area.GetAllVertexControls())
                    if (GetIsTagged(item))
                    {
                        item.ClearValue(OriginalXProperty);
                        item.ClearValue(OriginalYProperty);
                    }
            }

            //we finished the drag, release the mouse
#if WPF
            var element = sender as IInputElement;
            if (element != null)
            {
                element.MouseMove -= OnVertexDragging;
                element.ReleaseMouseCapture();
            }
#elif METRO
            var element = sender as FrameworkElement;
            if ( element != null )
            {
                element.PointerMoved -= OnDragging;
                element.ReleasePointerCapture(e.Pointer);
            }
#endif
        }

#if WPF

        private static void OnVertexDragging(object sender, System.Windows.Input.MouseEventArgs e)
#elif METRO
        private static void OnDragging( object sender, PointerRoutedEventArgs e )
#endif
        {
            var obj = sender as DependencyObject;
            if (!GetIsDragging(obj))
                return;

            var area = GetAreaFromObject(obj);
            var pos = GetPositionInArea(area, e);

            double horizontalChange = pos.X - GetOriginalMouseX(obj);
            double verticalChange = pos.Y - GetOriginalMouseY(obj);

            // Determine whether to use snapping
            bool snap = GetIsSnappingPredicate(obj)(obj);
            bool individualSnap = false;
            // Snap modifier functions to apply to the primary dragged object
            SnapModifierFunc snapXMod = null;
            SnapModifierFunc snapYMod = null;
            // Snap modifier functions to apply to other dragged objects if they snap individually instead of moving
            // the same amounts as the primary object.
            SnapModifierFunc individualSnapXMod = null;
            SnapModifierFunc individualSnapYMod = null;
            if (snap)
            {
                snapXMod = GetXSnapModifier(obj);
                snapYMod = GetYSnapModifier(obj);
                // If objects snap to grid individually instead of moving the same amount as the primary dragged object,
                // use the same snap modifier on each individual object.
                individualSnap = GetIsIndividualSnappingPredicate(obj)(obj);
                if (individualSnap)
                {
                    individualSnapXMod = snapXMod;
                    individualSnapYMod = snapYMod;
                }
            }

            if (GetIsTagged(obj))
            {
                // When the dragged item is a tagged item, we could be dragging a group of objects. If the dragged object is a vertex, it's
                // automatically the primary object of the drag. If the dragged object is an edge, prefer the source vertex, but accept the
                // target vertex as the primary object of the drag and start with that.
                var primaryDragVertex = obj as VertexControl;
                if (primaryDragVertex == null)
                {
                    var ec = obj as EdgeControl;
                    if (ec != null)
                        primaryDragVertex = ec.Source ?? ec.Target;

                    if (primaryDragVertex == null)
                    {
                        Debug.WriteLine("OnDragging() -> Tagged and dragged the wrong object?");
                        return;
                    }
                }
                UpdateCoordinates(area, primaryDragVertex, horizontalChange, verticalChange, snapXMod, snapYMod);

                if (!individualSnap)
                {
                    // When dragging groups of objects that all move the same amount (not snapping individually, but tracking with
                    // the movement of the primary dragged object), deterrmine how much offset the primary dragged object experienced
                    // and use that offset for the rest.
                    horizontalChange = GraphAreaBase.GetFinalX(primaryDragVertex) - GetOriginalX(primaryDragVertex);
                    verticalChange = GraphAreaBase.GetFinalY(primaryDragVertex) - GetOriginalY(primaryDragVertex);
                }

                foreach (var item in area.GetAllVertexControls())
                    if (!ReferenceEquals(item, primaryDragVertex) && GetIsTagged(item))
                        UpdateCoordinates(area, item, horizontalChange, verticalChange, individualSnapXMod, individualSnapYMod);
            }
            else UpdateCoordinates(area, obj, horizontalChange, verticalChange, snapXMod, snapYMod);
            e.Handled = true;
        }

        private static void UpdateVertexEdges(VertexControl vc)
        {
            if (vc?.Vertex != null)
            {
                var ra = vc.RootArea;
                if (ra == null) throw new GX_InvalidDataException("OnDragFinished() - IGraphControl object must always have RootArea property set!");
                if (ra.IsEdgeRoutingEnabled)
                {
                    ra.ComputeEdgeRoutesByVertex(vc);
#if WPF
                    vc.InvalidateVisual();
#elif METRO
                    vc.InvalidateArrange();
#endif
                }
            }
        }

        private static void UpdateCoordinates(GraphAreaBase area, DependencyObject obj, double horizontalChange, double verticalChange, SnapModifierFunc xSnapModifier, SnapModifierFunc ySnapModifier)
        {
            if (double.IsNaN(GraphAreaBase.GetX(obj)))
                GraphAreaBase.SetX(obj, 0, true);
            if (double.IsNaN(GraphAreaBase.GetY(obj)))
                GraphAreaBase.SetY(obj, 0, true);

            //move the object
            var x = GetOriginalX(obj) + horizontalChange;
            if (xSnapModifier != null)
                x = xSnapModifier(area, obj, x);
            GraphAreaBase.SetX(obj, x, true);

            var y = GetOriginalY(obj) + verticalChange;
            if (ySnapModifier != null)
                y = ySnapModifier(area, obj, y);
            GraphAreaBase.SetY(obj, y, true);

            if (GetUpdateEdgesOnMove(obj))
                UpdateVertexEdges(obj as VertexControl);

            //Debug.WriteLine("({0:##0.00000}, {1:##0.00000})", x, y);
        }

#if WPF

        private static Point GetPositionInArea(GraphAreaBase area, System.Windows.Input.MouseEventArgs e)
#elif METRO
        private static Windows.Foundation.Point GetPositionInArea(GraphAreaBase area, PointerRoutedEventArgs e)
#endif
        {
            if (area != null)
            {
#if WPF
                var pos = e.GetPosition(area);
#elif METRO
                var pos = e.GetCurrentPoint(area as UIElement).Position;
#endif
                return pos;
            }
            throw new GX_InvalidDataException("DragBehavior.GetPositionInArea() - The input element must be a child of a GraphAreaBase.");
        }

        private static GraphAreaBase GetAreaFromObject(object obj)
        {
            GraphAreaBase area = null;

            if (obj is VertexControl)
                area = ((VertexControl)obj).RootArea;
            else if (obj is EdgeControl)
                area = ((EdgeControl)obj).RootArea;
            else if (obj is DependencyObject)
                area = VisualTreeHelperEx.FindAncestorByType((DependencyObject)obj, typeof(GraphAreaBase), false) as GraphAreaBase;

            return area;
        }
    }
}