using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using GraphX.Common.Enums;
using GraphX.Common.Exceptions;
using GraphX.Controls.Models;
using GraphX.Common.Interfaces;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual vertex control
    /// </summary>
    [Serializable]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Pressed")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerLeave")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(IVertexLabelControl))]
    [TemplatePart(Name = "PART_vcproot", Type = typeof(Panel))]
    public class VertexControl : VertexControlBase
    {
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
            foreach (var item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
                UpdateEventhandling(item);
        }

        #region Position trace feature

        private ChangeMonitor? _xChangeMonitor;
        private ChangeMonitor? _yChangeMonitor;

        internal void UpdatePositionTraceState()
        {
            if (EventOptions is {PositionChangeNotification: true})
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
            if (ShowLabel)
                VertexLabelControl?.UpdatePosition();
            OnPositionChanged(new Point(), GetPosition());
        }

        #endregion Position trace feature

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
                    if (EventOptions is {MouseClickEnabled: true})
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
                    if (EventOptions is {MouseDoubleClickEnabled: true}) MouseDoubleClick += VertexControl_MouseDoubleClick;
                    else MouseDoubleClick -= VertexControl_MouseDoubleClick;
                    break;

                case EventType.MouseMove:
                    if (EventOptions is {MouseMoveEnabled: true}) MouseMove += VertexControl_MouseMove;
                    else MouseMove -= VertexControl_MouseMove;
                    break;

                case EventType.MouseEnter:
                    if (EventOptions is {MouseEnterEnabled: true}) MouseEnter += VertexControl_MouseEnter;
                    else MouseEnter -= VertexControl_MouseEnter;
                    break;

                case EventType.MouseLeave:
                    if (EventOptions is {MouseLeaveEnabled: true}) MouseLeave += VertexControl_MouseLeave;
                    else MouseLeave -= VertexControl_MouseLeave;
                    break;

                case EventType.PositionChangeNotify:
                    UpdatePositionTraceState();
                    break;
            }
            MouseUp -= VertexControl_MouseUp;
            MouseUp += VertexControl_MouseUp;
        }

        private void VertexControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_clickTrack)
                return;

            var curPoint = RootArea != null ? Mouse.GetPosition(RootArea) : new Point();

            if (curPoint != _clickTrackPoint)
                _clickTrack = false;
        }

        private void VertexControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnVertexMouseUp(this, e, Keyboard.Modifiers);
                if (_clickTrack)
                {
                    RaiseEvent(new RoutedEventArgs(ClickEvent, this));
                    RootArea.OnVertexClicked(this, e, Keyboard.Modifiers);
                }
            }
            _clickTrack = false;
            e.Handled = true;
        }

        private void VertexControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexDoubleClick(this, e);
            //e.Handled = true;
        }

        private void VertexControl_Down(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexSelected(this, e, Keyboard.Modifiers);
            _clickTrack = true;
            _clickTrackPoint = RootArea != null ? Mouse.GetPosition(RootArea) : new Point();
            e.Handled = true;
        }

        #endregion Event tracing

        #region Click Event

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VertexControl));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        #endregion Click Event

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

            public event Changed? ChangeDetected;

            public static readonly DependencyProperty MonitorForChangeProperty =
                DependencyProperty.Register(nameof(MonitorForChange), typeof(object), typeof(ChangeMonitor), new PropertyMetadata(null, MonitoredPropertyChanged));

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
                changeDetected?.Invoke(cm, new EventArgs());
            }
        }

        #endregion ChangeMonitor class

        /// <summary>
        /// Gets the root element which hosts VCPs so you can add them at runtime. Requires Panel-descendant template item defined named PART_vcproot.
        /// </summary>
        public Panel? VCPRoot { get; protected set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null) return;
            VertexLabelControl = VertexLabelControl ?? FindDescendant<IVertexLabelControl>("PART_vertexLabel");

            VCPRoot ??= FindDescendant<Panel>("PART_vcproot");

            if (VertexLabelControl != null)
            {
                if (ShowLabel) VertexLabelControl.Show(); else VertexLabelControl.Hide();
                UpdateLayout();
                VertexLabelControl.UpdatePosition();
            }

            VertexConnectionPointsList = this.FindDescendantsOfType<IVertexConnectionPoint>().ToList();
            if (VertexConnectionPointsList.GroupBy(x => x.Id).Count(group => @group.Count() > 1) > 0)
                throw new GX_InvalidDataException("Vertex connection points in VertexControl template must have unique Id!");
        }

        #region Events handling

        private void VertexControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseLeave(this, e);
            VisualStateManager.GoToState(this, "PointerLeave", true);
        }

        private void VertexControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseEnter(this, e);
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        private void VertexControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootArea != null)
                RootArea.OnVertexMouseMove(this, e);
        }

        #endregion Events handling

        /// <summary>
        /// Cleans all potential memory-holding code
        /// </summary>
        public override void Clean()
        {
            Vertex = null;
            RootArea = null!;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            VertexLabelControl = null;

            if (EventOptions != null)
            {
                EventOptions.PositionChangeNotification = false;
                EventOptions.Clean();
            }
        }

        /// <summary>
        /// Gets Vertex data as specified class
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        public T GetDataVertex<T>() where T : IGraphXVertex
        {
            return (T)Vertex!;
        }
    }
}