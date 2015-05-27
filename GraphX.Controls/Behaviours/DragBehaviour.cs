using System.Diagnostics;
#if WPF
using System.Windows;
#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
#endif
using GraphX.PCL.Common.Exceptions;

namespace GraphX.Controls
{
    public static class DragBehaviour
    {
        #region Attached DPs
        public static readonly DependencyProperty IsDragEnabledProperty = DependencyProperty.RegisterAttached("IsDragEnabled", typeof(bool), typeof(DragBehaviour), new PropertyMetadata(false, OnIsDragEnabledPropertyChanged));
        public static readonly DependencyProperty UpdateEdgesOnMoveProperty = DependencyProperty.RegisterAttached("UpdateEdgesOnMove", typeof(bool), typeof(DragBehaviour), new PropertyMetadata(false));
        public static readonly DependencyProperty IsTaggedProperty = DependencyProperty.RegisterAttached("IsTagged", typeof(bool), typeof(DragBehaviour), new PropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(DragBehaviour), new PropertyMetadata(false));
        private static readonly DependencyProperty OriginalXProperty = DependencyProperty.RegisterAttached("OriginalX", typeof(double), typeof(DragBehaviour), new PropertyMetadata(0.0));
        private static readonly DependencyProperty OriginalYProperty = DependencyProperty.RegisterAttached("OriginalY", typeof(double), typeof(DragBehaviour), new PropertyMetadata(0.0));
        #endregion

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
        #endregion

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
                element.MouseLeftButtonDown += OnDragStarted;
                element.MouseLeftButtonUp += OnDragFinished;
#elif METRO
                element.PointerPressed += OnDragStarted;
				element.PointerReleased += OnDragFinished;
#endif
            }
            else
            {
                //unregister the event handlers
#if WPF
                element.MouseLeftButtonDown -= OnDragStarted;
                element.MouseLeftButtonUp -= OnDragFinished;
#elif METRO
                element.PointerPressed -= OnDragStarted;
				element.PointerReleased -= OnDragFinished;
#endif
            }
        }
        #endregion

        private static Point _scale = new Point(1, 1);
#if WPF
        private static void OnDragStarted(object sender, System.Windows.Input.MouseButtonEventArgs e)
#elif METRO
		private static void OnDragStarted( object sender, PointerRoutedEventArgs e )
#endif
        {
            var obj = sender as DependencyObject;
            //we are starting the drag
            SetIsDragging(obj, true);

#if WPF
            var pos = e.GetPosition(obj as IInputElement);
#elif METRO
			var pos = e.GetCurrentPoint(obj as UIElement).Position;
#endif

            //save the position of the mouse to the start position
            SetOriginalX(obj, pos.X);
            SetOriginalY(obj, pos.Y);

            //capture the mouse
#if WPF
            var element = obj as IInputElement;
            if (element != null)
            {
                element.CaptureMouse();
                element.MouseMove += OnDragging;
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
        private static void OnDragFinished(object sender, System.Windows.Input.MouseButtonEventArgs e)
#elif METRO
		private static void OnDragFinished( object sender, PointerRoutedEventArgs e )
#endif
        {
            UpdateVertexEdges(sender as VertexControl);

            var obj = (DependencyObject)sender;
            SetIsDragging(obj, false);
            obj.ClearValue(OriginalXProperty);
            obj.ClearValue(OriginalYProperty);

            //we finished the drag, release the mouse
#if WPF
            var element = sender as IInputElement;
            if (element != null)
            {
                element.MouseMove -= OnDragging;
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
            e.Handled = true;
        }

#if WPF
        private static void OnDragging(object sender, System.Windows.Input.MouseEventArgs e)
#elif METRO
		private static void OnDragging( object sender, PointerRoutedEventArgs e )
#endif
        {
            var obj = sender as DependencyObject;
            if (!GetIsDragging(obj))
                return;

#if WPF
            Point pos = e.GetPosition(obj as IInputElement);
#elif METRO
            Point pos = e.GetCurrentPoint(obj as UIElement).Position;
#endif
            double horizontalChange = (pos.X - GetOriginalX(obj)) * _scale.X;
            double verticalChange = (pos.Y - GetOriginalY(obj)) * _scale.Y;
            if (GetIsTagged(obj))
            {
                var vc = obj as VertexControl;
                if (vc == null)
                {
                    Debug.WriteLine("OnDragging() -> Tagged and dragged the wrong object?");
                    return;
                }
                foreach (var item in vc.RootArea.GetAllVertexControls())
                    if (GetIsTagged(item))
                        UpdateCoordinates(item, horizontalChange, verticalChange);
            }
            else UpdateCoordinates(obj, horizontalChange, verticalChange);
            e.Handled = true;
        }

        private static void UpdateVertexEdges(VertexControl vc)
        {
            if (vc != null && vc.Vertex != null)
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

        private static void UpdateCoordinates(DependencyObject obj, double horizontalChange, double verticalChange)
        {

            if (double.IsNaN(GraphAreaBase.GetX(obj)))
                GraphAreaBase.SetX(obj, 0, true);
            if (double.IsNaN(GraphAreaBase.GetY(obj)))
                GraphAreaBase.SetY(obj, 0, true);

            //move the object
            var x = GraphAreaBase.GetX(obj) + horizontalChange;
            GraphAreaBase.SetX(obj, x, true);
            var y = GraphAreaBase.GetY(obj) + verticalChange;
            GraphAreaBase.SetY(obj, y, true);
            if (GetUpdateEdgesOnMove(obj))
                UpdateVertexEdges(obj as VertexControl);

        }
    }
}
