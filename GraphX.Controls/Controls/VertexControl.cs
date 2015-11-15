using System;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
#elif METRO
using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Interfaces;


namespace GraphX.Controls
{
    /// <summary>
    /// Visual vertex control
    /// </summary>
#if WPF
    [Serializable]
#endif
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Pressed")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerLeave")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(IVertexLabelControl))]
    public class VertexControl: VertexControlBase
    {
#if WPF
        static VertexControl()
        {
            //override the StyleKey Property
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VertexControl), new FrameworkPropertyMetadata(typeof(VertexControl)));
        }

        /// <summary>
        /// Create vertex visual control
        /// </summary>
        /// <param name="vertexData">Vertex data object</param>
        /// <param name="tracePositionChange">Listen for the vertex position changed events and fire corresponding event</param>
        /// <param name="bindToDataObject">Bind DataContext to the Vertex data. True by default. </param>
        public VertexControl(object vertexData, bool tracePositionChange = true, bool bindToDataObject = true)
        {
            if (bindToDataObject) DataContext = vertexData;
            Vertex = vertexData;

            EventOptions = new VertexEventOptions(this) { PositionChangeNotification = tracePositionChange };
            foreach(var item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
                UpdateEventhandling(item);
        }


        #region Position trace feature


        private ChangeMonitor _xChangeMonitor;
        private ChangeMonitor _yChangeMonitor;
        internal void UpdatePositionTraceState()
        {
            if (EventOptions.PositionChangeNotification)
            {
                if (_xChangeMonitor == null)
                {
                    _xChangeMonitor = new ChangeMonitor();
                    _xChangeMonitor.Bind(this, GraphAreaBase.XProperty);
                    _xChangeMonitor.ChangeDetected += changeMonitor_ChangeDetected;
                }
                if (_yChangeMonitor == null)
                {
                    _yChangeMonitor = new ChangeMonitor();
                    _yChangeMonitor.Bind(this, GraphAreaBase.YProperty);
                    _yChangeMonitor.ChangeDetected += changeMonitor_ChangeDetected;
                }
            }
            else
            {
                if (_xChangeMonitor != null)
                {
                    _xChangeMonitor.ChangeDetected -= changeMonitor_ChangeDetected;
                    _xChangeMonitor.Unbind();
                    _xChangeMonitor = null;
                }
                if (_yChangeMonitor != null)
                {
                    _yChangeMonitor.ChangeDetected -= changeMonitor_ChangeDetected;
                    _yChangeMonitor.Unbind();
                    _yChangeMonitor = null;
                }
            }
        }

        private void changeMonitor_ChangeDetected(object source, EventArgs args)
        {
            if(ShowLabel && VertexLabelControl != null)
                VertexLabelControl.UpdatePosition();
            OnPositionChanged(new Point(), GetPosition());
        }

        #endregion

        public T FindDescendant<T>(string name)
        {
            return (T)Template.FindName(name, this);
        }

        #region Event tracing

        private bool _clickTrack;
		private Point _clickTrackPoint;

        internal void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled)
                    {
                        MouseDown += VertexControl_Down;
                        PreviewMouseMove += VertexControl_PreviewMouseMove;
                    }
                    else
                    {
                        MouseDown -= VertexControl_Down;
                        PreviewMouseMove -= VertexControl_PreviewMouseMove;
                    }
                    break;
                case EventType.MouseDoubleClick:
                    if (EventOptions.MouseDoubleClickEnabled) MouseDoubleClick += VertexControl_MouseDoubleClick;
                    else MouseDoubleClick -= VertexControl_MouseDoubleClick;
                    break;
                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled) MouseMove += VertexControl_MouseMove;
                    else MouseMove -= VertexControl_MouseMove;
                    break;
                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled) MouseEnter += VertexControl_MouseEnter;
                    else MouseEnter -= VertexControl_MouseEnter;
                    break;
                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled) MouseLeave += VertexControl_MouseLeave;
                    else MouseLeave -= VertexControl_MouseLeave;
                    break;
                case EventType.PositionChangeNotify:
                    UpdatePositionTraceState();
                    break;
            }
            MouseUp += VertexControl_MouseUp;
        }

        void VertexControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
			if (!_clickTrack)
				return;

            var curPoint = RootArea != null ? Mouse.GetPosition(RootArea) : new Point();

			if (curPoint != _clickTrackPoint)
				_clickTrack = false;
        }

        void VertexControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnVertexMouseUp(this, e, Keyboard.Modifiers);
                if (_clickTrack)
                {
                    RaiseClick();
                }
            }
            _clickTrack = false;
            e.Handled = true;
        }

                void VertexControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexDoubleClick(this, e);
            //e.Handled = true;
        }

        void VertexControl_Down(object sender, MouseButtonEventArgs e)
        {
			if (RootArea != null && Visibility == Visibility.Visible)
				RootArea.OnVertexSelected(this, e, Keyboard.Modifiers);
            _clickTrack = true;
			_clickTrackPoint = RootArea != null ? Mouse.GetPosition(RootArea) : new Point();
            e.Handled = true;
        }
        #endregion

        #region Click Event

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VertexControl));
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        // This method raises the PageNavigation event
        private void RaiseClick()
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }

        #endregion

        #region ChangeMonitor class

        /// <summary>
        /// This class is used to monitor for changes on the specified property of the specified control.
        /// </summary>
        private class ChangeMonitor : DependencyObject
        {
            public void Bind(UIElement el, DependencyProperty property)
            {
                var b = new Binding
                {
                    Path = new PropertyPath(property),
                    Source = el
                };
                BindingOperations.SetBinding(this, MonitorForChangeProperty, b);
            }

            public void Unbind()
            {
                BindingOperations.ClearBinding(this, MonitorForChangeProperty);
            }

            public delegate void Changed(object source, EventArgs args);

            public event Changed ChangeDetected;

            public static readonly DependencyProperty MonitorForChangeProperty =
                DependencyProperty.Register("MonitorForChange", typeof(object), typeof(ChangeMonitor), new PropertyMetadata(null, MonitoredPropertyChanged));

            public object MonitorForChange
            {
                get { return GetValue(MonitorForChangeProperty); }
                set { SetValue(MonitorForChangeProperty, value); }
            }

            private static void MonitoredPropertyChanged(object source, DependencyPropertyChangedEventArgs args)
            {
                var cm = source as ChangeMonitor;
                if (cm == null)
                {
                    return;
                }
                var changeDetected = cm.ChangeDetected;
                if (changeDetected != null)
                {
                    changeDetected(cm, new EventArgs());
                }
            }
        }

        #endregion
#elif METRO
        #region Attached property tracer
        private static readonly DependencyProperty TestXProperty =
            DependencyProperty.Register("TestX", typeof(double), typeof(VertexControl), new PropertyMetadata(0, Testxchanged));

        private static readonly DependencyProperty TestYProperty =
            DependencyProperty.Register("TestY", typeof(double), typeof(VertexControl), new PropertyMetadata(0, Testychanged));

        private static void Testychanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vc = d as IPositionChangeNotify;
            if (vc != null)
                vc.OnPositionChanged();
        }

        private static void Testxchanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vc = d as IPositionChangeNotify;
            if (vc != null)
                vc.OnPositionChanged();
        }
        #endregion

        /// <summary>
        /// Create vertex visual control
        /// </summary>
        /// <param name="vertexData">Vertex data object</param>
        /// <param name="bindToDataObject">Bind DataContext to the Vertex data. True by default. </param>
        public VertexControl(object vertexData,  bool bindToDataObject = true)
        {
            DefaultStyleKey = typeof (VertexControl);
            if (bindToDataObject) DataContext = vertexData;
            Vertex = vertexData;

            EventOptions = new VertexEventOptions(this);
            foreach(var item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
                UpdateEventhandling(item);

            IsEnabledChanged += (sender, args) => VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);

            var xBinding = new Binding
            {
                Path = new PropertyPath("(Canvas.Left)"),
                Source = this
            };
            SetBinding(TestXProperty, xBinding);
            var yBinding = new Binding
            {
                Path = new PropertyPath("(Canvas.Top)"),
                Source = this
            };
            SetBinding(TestYProperty, yBinding);
        }

        public T FindDescendant<T>(string name)
        {
            return (T)(object)this.FindDescendantByName(name);
        }

        protected internal virtual void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled) PointerPressed += VertexControl_Down;
                    else PointerPressed -= VertexControl_Down;
                    break;
                case EventType.MouseDoubleClick:
                    // if (EventOptions.MouseDoubleClickEnabled) Poi += VertexControl_MouseDoubleClick;
                    // else MouseDoubleClick -= VertexControl_MouseDoubleClick;
                    break;
                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled) PointerMoved += VertexControl_MouseMove;
                    else PointerMoved -= VertexControl_MouseMove;
                    break;
                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled) PointerEntered += VertexControl_MouseEnter;
                    else PointerEntered -= VertexControl_MouseEnter;
                    break;
                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled) PointerExited += VertexControl_MouseLeave;
                    else PointerExited -= VertexControl_MouseLeave;
                    break;
            }
        }

        void VertexControl_Down(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexSelected(this, e, null);
            //e.Handled = true;
            VisualStateManager.GoToState(this, "Pressed", true);
        }
#endif

#if WPF
        public 
#elif METRO
        protected
#endif
        override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null) return;
            VertexLabelControl = VertexLabelControl ?? FindDescendant<IVertexLabelControl>("PART_vertexLabel");

            if (VertexLabelControl != null)
            {
                if(ShowLabel) VertexLabelControl.Show(); else VertexLabelControl.Hide();
                UpdateLayout();
                VertexLabelControl.UpdatePosition();
            }

            VertexConnectionPointsList = this.FindDescendantsOfType<IVertexConnectionPoint>().ToList();
            if (VertexConnectionPointsList.GroupBy(x => x.Id).Count(group => @group.Count() > 1) > 0)
                throw new GX_InvalidDataException("Vertex connection points in VertexControl template must have unique Id!");
        }

        #region Events handling

		

        void VertexControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseLeave(this, e);
            VisualStateManager.GoToState(this, "PointerLeave", true);
        }

        void VertexControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseEnter(this, e);
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        void VertexControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootArea != null)
                RootArea.OnVertexMouseMove(this, e);
        }

        #endregion

        /// <summary>
        /// Cleans all potential memory-holding code
        /// </summary>
        public override void Clean()
        {
            Vertex = null;
            RootArea = null;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            VertexLabelControl = null;

            if (EventOptions != null)
            {
#if WPF
                EventOptions.PositionChangeNotification = false;
#endif
                EventOptions.Clean();
            }
        }

        /// <summary>
        /// Gets Vertex data as specified class
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        public T GetDataVertex<T>() where T : IGraphXVertex
        {
            return (T)Vertex;
        }   
    }
}