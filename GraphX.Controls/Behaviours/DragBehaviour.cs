using System.Diagnostics;
using System.Windows;
using GraphX.PCL.Common.Exceptions;

namespace GraphX.WPF.Controls
{
	public static class DragBehaviour
	{
		#region Attached DPs
        public static readonly DependencyProperty IsDragEnabledProperty = DependencyProperty.RegisterAttached("IsDragEnabled", typeof(bool), typeof(DragBehaviour), new UIPropertyMetadata(false, OnIsDragEnabledPropertyChanged));
        public static readonly DependencyProperty UpdateEdgesOnMoveProperty = DependencyProperty.RegisterAttached("UpdateEdgesOnMove", typeof(bool), typeof(DragBehaviour), new UIPropertyMetadata(false));
        public static readonly DependencyProperty IsTaggedProperty = DependencyProperty.RegisterAttached("IsTagged", typeof(bool), typeof(DragBehaviour), new UIPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(DragBehaviour), new UIPropertyMetadata(false));
        private static readonly DependencyPropertyKey OriginalXPropertyKey = DependencyProperty.RegisterAttachedReadOnly("OriginalX", typeof(double), typeof(DragBehaviour), new UIPropertyMetadata(0.0));
		private static readonly DependencyPropertyKey OriginalYPropertyKey = DependencyProperty.RegisterAttachedReadOnly( "OriginalY", typeof( double ), typeof( DragBehaviour ), new UIPropertyMetadata( 0.0 ) );
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

		public static bool GetIsDragEnabled( DependencyObject obj )
		{
			return (bool)obj.GetValue( IsDragEnabledProperty );
		}

		public static void SetIsDragEnabled( DependencyObject obj, bool value )
		{
			obj.SetValue( IsDragEnabledProperty, value );
		}

		public static bool GetIsDragging( DependencyObject obj )
		{
			return (bool)obj.GetValue( IsDraggingProperty );
		}

		public static void SetIsDragging( DependencyObject obj, bool value )
		{
			obj.SetValue( IsDraggingProperty, value );
		}

		private static double GetOriginalX( DependencyObject obj )
		{
			return (double)obj.GetValue( OriginalXPropertyKey.DependencyProperty );
		}

		private static void SetOriginalX( DependencyObject obj, double value )
		{
			obj.SetValue( OriginalXPropertyKey, value );
		}

		private static double GetOriginalY( DependencyObject obj )
		{
			return (double)obj.GetValue( OriginalYPropertyKey.DependencyProperty );
		}

		private static void SetOriginalY( DependencyObject obj, double value )
		{
			obj.SetValue( OriginalYPropertyKey, value );
		}
		#endregion

		#region PropertyChanged callbacks
		private static void OnIsDragEnabledPropertyChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
		{
			var element = obj as FrameworkElement;
			FrameworkContentElement contentElement = null;
			if ( element == null )
			{
				contentElement = obj as FrameworkContentElement;
				if ( contentElement == null )
					return;
			}

			if ( e.NewValue is bool == false )
				return;

			if ( (bool)e.NewValue )
			{
				//register the event handlers
				if ( element != null )
				{
					//registering on the FrameworkElement
					element.MouseLeftButtonDown += OnDragStarted;
					element.MouseLeftButtonUp += OnDragFinished;
				}
				else
				{
					//registering on the FrameworkContentElement
					contentElement.MouseLeftButtonDown += OnDragStarted;
					contentElement.MouseLeftButtonUp += OnDragFinished;
				}
                //Debug.WriteLine("DragBehaviour registered.", "DEBUG");
			}
			else
			{
				//unregister the event handlers
				if ( element != null )
				{
					//unregistering on the FrameworkElement
					element.MouseLeftButtonDown -= OnDragStarted;
					element.MouseLeftButtonUp -= OnDragFinished;
				}
				else
				{
					//unregistering on the FrameworkContentElement
					contentElement.MouseLeftButtonDown -= OnDragStarted;
					contentElement.MouseLeftButtonUp -= OnDragFinished;
				}
                //Debug.WriteLine("DragBehaviour unregistered.", "DEBUG");
			}
		}
		#endregion

        private static Point scale = new Point(1, 1);
		private static void OnDragStarted( object sender, System.Windows.Input.MouseButtonEventArgs e )
		{
			var obj = sender as DependencyObject;
            var vc = obj as VertexControl;
           /* if (vc != null && vc.RootArea.GetAllVertexControls().Length == 1)
            {
                Debug.WriteLine("OnDragStarted() -> we have only 1 vertex. Dragging is prohibited.");
                return;
            }*/
			//we are starting the drag
			SetIsDragging( obj, true );

			Point pos = e.GetPosition( obj as IInputElement );

			//save the position of the mouse to the start position
			SetOriginalX( obj, pos.X );
			SetOriginalY( obj, pos.Y );

            //Debug.WriteLine("Drag started on object: " + obj, "DEBUG");

			//capture the mouse
			var element = obj as FrameworkElement;
			if ( element != null )
			{
				element.CaptureMouse();
				element.MouseMove += OnDragging;
                /*ScaleTransform scaleTransform = null;
                if (element.RenderTransform is TransformGroup)
                {
                    var transformGroup = element.RenderTransform as TransformGroup;
                    if (transformGroup != null)
                    {
                        foreach (var item in transformGroup.Children)
                            if (item is ScaleTransform)
                            {
                                scaleTransform = item as ScaleTransform; break;
                            }
                    }
                }
                else
                {
                    scaleTransform = element.RenderTransform as ScaleTransform;
                }

                if (scaleTransform != null)
                {
                    scale = new Point(scaleTransform.ScaleX, scaleTransform.ScaleY);
                }*/
			}
			else
			{
				var contentElement = obj as FrameworkContentElement;
				if ( contentElement == null )
                    throw new GX_InvalidDataException("The control must be a descendent of the FrameworkElement or FrameworkContentElement!");
				contentElement.CaptureMouse();
				contentElement.MouseMove += OnDragging;
			}
		    e.Handled = false;
		}

		private static void OnDragFinished( object sender, System.Windows.Input.MouseButtonEventArgs e )
		{
            UpdateVertexEdges(sender as VertexControl);

			var obj = (DependencyObject)sender;
			SetIsDragging( obj, false );
			obj.ClearValue( OriginalXPropertyKey );
			obj.ClearValue( OriginalYPropertyKey );

           // Debug.WriteLine("Drag finished on object: " + obj, "DEBUG");

			//we finished the drag, release the mouse
			var element = sender as FrameworkElement;
			if ( element != null )
			{
				element.MouseMove -= OnDragging;
				element.ReleaseMouseCapture();
			}
			else
			{
				var contentElement = sender as FrameworkContentElement;
				if (contentElement == null)
                    throw new GX_InvalidDataException("The control must be a descendent of the FrameworkElement or FrameworkContentElement!");
				contentElement.MouseMove -= OnDragging;
				contentElement.ReleaseMouseCapture();
			}
		    e.Handled = true;
		}

		private static void OnDragging( object sender, System.Windows.Input.MouseEventArgs e )
		{
			var obj = sender as DependencyObject;
			if ( !GetIsDragging( obj ) )
				return;

            Point pos = e.GetPosition(obj as IInputElement);
            double horizontalChange = (pos.X - GetOriginalX(obj)) *scale.X;
            double verticalChange = (pos.Y - GetOriginalY(obj)) *scale.Y;
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


            //SetOriginalX(obj, pos.X);
            //SetOriginalY(obj, pos.Y);


		    e.Handled = true;
		}

        private static void UpdateVertexEdges(VertexControl vc)
        {
            if (vc != null && vc.Vertex != null)
            {
                var ra = vc.RootArea;
                if (ra == null) throw new GX_InvalidDataException("OnDragFinished() - IGraphControl object must always have RootArea property set!");
                //if (ra.IsEdgeRoutingEnabled) ra.ComputeEdgeRoutesByVertex(vc);
                //foreach (var item in ra.GetRelatedControls(vc, GraphControlType.Edge, EdgesType.All))
                //{
                    if (ra.IsEdgeRoutingEnabled)
                    {
                        ra.ComputeEdgeRoutesByVertex(vc);
                        vc.InvalidateVisual();
                    }
               // }
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
