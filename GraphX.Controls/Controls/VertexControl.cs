using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Interfaces;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual vertex control
    /// </summary>
    [Serializable]
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(IVertexLabelControl))]
    public class VertexControl: VertexControlBase
    {
        static VertexControl()
        {
            //override the StyleKey Property
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VertexControl), new FrameworkPropertyMetadata(typeof(VertexControl)));
        }

        /// <summary>
        /// Gets Vertex data as specified class
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        public T GetDataVertex<T>() where T: IGraphXVertex
        {
            return (T)Vertex;
        }        

        #region Position trace feature


        private ChangeMonitor _xChangeMonitor = null;
        private ChangeMonitor _yChangeMonitor = null;
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
                    _xChangeMonitor.Unbind(this);
                    _xChangeMonitor = null;
                }
                if (_yChangeMonitor == null)
                {
                    _yChangeMonitor.ChangeDetected -= changeMonitor_ChangeDetected;
                    _yChangeMonitor.Unbind(this);
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

            SizeChanged += VertexControl_SizeChanged;
        }

        void VertexControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*if (ShowLabel && VertexLabelControl != null)
                VertexLabelControl.UpdatePosition();
            OnPositionChanged(new Point(), GetPosition());*/
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null) return;
            VertexLabelControl = Template.FindName("PART_vertexLabel", this) as IVertexLabelControl;

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

		private bool clickTrack = false;
		private Point clickTrackPoint;

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
			if (!clickTrack)
				return;

			Point curPoint;
			if (RootArea != null)
				curPoint = Mouse.GetPosition(RootArea);
			else
				curPoint = new Point();

			if (curPoint != clickTrackPoint)
				clickTrack = false;
        }

        void VertexControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnVertexMouseUp(this, e, Keyboard.Modifiers);
                if (clickTrack)
                {
                    RaiseClick();
                }
            }
            clickTrack = false;
            e.Handled = true;
        }

        void VertexControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseLeave(this);
            //e.Handled = true;
        }

        void VertexControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseEnter(this);
           // e.Handled = true;
        }

        void VertexControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootArea != null)
                RootArea.OnVertexMouseMove(this);
        }

        void VertexControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexDoubleClick(this);
            //e.Handled = true;
        }

        void VertexControl_Down(object sender, MouseButtonEventArgs e)
        {
			if (RootArea != null && Visibility == Visibility.Visible)
				RootArea.OnVertexSelected(this, e, Keyboard.Modifiers);
            clickTrack = true;
			clickTrackPoint = RootArea != null ? Mouse.GetPosition(RootArea) : new Point();
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

        /// <summary>
        /// Cleans all potential memory-holding code
        /// </summary>
        public override void Clean()
        {
            Vertex = null;
            RootArea = null;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            if (EventOptions != null)
            {
                EventOptions.PositionChangeNotification = false;
                EventOptions.Clean();
            }
        }

        #region ChangeMonitor class

        /// <summary>
        /// This class is used to monitor for changes on the specified property of the specified control.
        /// </summary>
        private class ChangeMonitor : DependencyObject
        {
            public ChangeMonitor()
            {
            }

            public void Bind(UIElement el, DependencyProperty property)
            {
                Binding b = new Binding();
                b.Path = new PropertyPath(property);
                b.Source = el;
                BindingOperations.SetBinding(this, MonitorForChangeProperty, b);
            }

            public void Unbind(UIElement el)
            {
                BindingOperations.ClearBinding(this, MonitorForChangeProperty);
            }

            public delegate void Changed(object source, EventArgs args);

            public event Changed ChangeDetected;

            public static readonly DependencyProperty MonitorForChangeProperty =
                DependencyProperty.Register("MonitorForChange", typeof(object), typeof(ChangeMonitor), new PropertyMetadata(null, MonitoredPropertyChanged));

            public object MonitorForChange
            {
                get { return (object)GetValue(MonitorForChangeProperty); }
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
    }
}