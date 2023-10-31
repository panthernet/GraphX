﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphX.Controls.Models;
using GraphX.Common;
using GraphX.Common.Enums;
using GraphX.Common.Interfaces;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual edge control
    /// </summary>
    [Serializable]
    public class EdgeControl : EdgeControlBase
    {
        #region Dependency Properties

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(nameof(StrokeThickness), typeof(double),
                                                                                              typeof(EdgeControl),
                                                                                              new PropertyMetadata(5.0));


       /// <summary>
       /// Custom edge thickness
       /// </summary>
       public double StrokeThickness
       {
           get { return (double)GetValue(StrokeThicknessProperty); }
           set { SetValue(StrokeThicknessProperty, value); }
       }


       private static readonly DependencyProperty IsSelfLoopedProperty = DependencyProperty.Register(nameof(IsSelfLooped), typeof(bool), typeof(EdgeControl), new PropertyMetadata(false));

       private bool IsSelfLoopedInternal => Source != null && Target != null && Source.Vertex == Target.Vertex;

        /// <summary>
       /// Gets if this edge is self looped (have same Source and Target)
       /// </summary>
       public override bool IsSelfLooped
       {
           get { return IsSelfLoopedInternal; }
           protected set { SetValue(IsSelfLoopedProperty, value); }
       }

        #endregion


        #region public Clean()
        public override void Clean()
        {
            Source = null;
            Target = null;
            Edge = null;
            RootArea = null;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            Linegeometry = null;
            LinePathObject = null;
            SelfLoopIndicator = null;
            EdgeLabelControls.ForEach(l=> l.Dispose());
            EdgeLabelControls.Clear();

            if (EdgePointerForSource != null)
            {
                EdgePointerForSource.Dispose();
                EdgePointerForSource = null;
            }
            if (EdgePointerForTarget != null)
            {
                EdgePointerForTarget.Dispose();
                EdgePointerForTarget = null;
            }
            EventOptions?.Clean();
        }
        #endregion

        #region Vertex position tracing
        private bool _sourceTrace;
        private bool _targetTrace;
        private VertexControl _oldSource;
        private VertexControl _oldTarget;

        protected override void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SourceChanged();
        }

        protected override void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TargetChanged();
        }

        private void SourceChanged()
        {
            // Only proceed if not in design mode
            if (EventOptions == null)
                return;

            if (_oldSource != null)
            {
                _oldSource.PositionChanged -= source_PositionChanged;
                _oldSource.SizeChanged -= Source_SizeChanged;
                _oldSource.EventOptions.PositionChangeNotification = _sourceTrace;
            }
            _oldSource = Source;
            if (Source != null) 
            {
                _sourceTrace = Source.EventOptions.PositionChangeNotification;
                Source.EventOptions.PositionChangeNotification = true;
                Source.PositionChanged += source_PositionChanged;
                Source.SizeChanged += Source_SizeChanged;
            }
            IsSelfLooped = IsSelfLoopedInternal;
            UpdateSelfLoopedEdgeData();
        }
        private void TargetChanged()
        {
            // Only proceed if not in design mode
            if (EventOptions == null)
                return;

            if (_oldTarget != null)
            {
                _oldTarget.PositionChanged -= source_PositionChanged;
                _oldTarget.SizeChanged -= Source_SizeChanged;
                _oldTarget.EventOptions.PositionChangeNotification = _targetTrace;
            }
            _oldTarget = Target;
            if (Target != null)
            {
                _targetTrace = Target.EventOptions.PositionChangeNotification;
                Target.EventOptions.PositionChangeNotification = true;
                Target.PositionChanged += source_PositionChanged;
                Target.SizeChanged += Source_SizeChanged;
            }
            IsSelfLooped = IsSelfLoopedInternal;
            UpdateSelfLoopedEdgeData();
        }

        private void source_PositionChanged(object sender, EventArgs e)
        {
            //update edge on any connected vertex position changes
            UpdateEdge();
        }

        void Source_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateEdge();
        }
        #endregion

        static EdgeControl()
        {
            //override the StyleKey
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EdgeControl), new FrameworkPropertyMetadata(typeof(EdgeControl)));
        }

        private bool _clickTrack;
        private Point _clickTrackPoint;

        internal void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled)
                    {
                        MouseDown += EdgeControl_MouseDown;
                        PreviewMouseMove += EdgeControl_PreviewMouseMove;
                    }
                    else
                    {
                        MouseDown -= EdgeControl_MouseDown;
                        PreviewMouseMove -= EdgeControl_PreviewMouseMove;
                    }
                    break;
                case EventType.MouseDoubleClick:
                    if (EventOptions.MouseDoubleClickEnabled) MouseDoubleClick += EdgeControl_MouseDoubleClick;
                    else MouseDoubleClick -= EdgeControl_MouseDoubleClick;
                    break;
                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled) MouseEnter += EdgeControl_MouseEnter;
                    else MouseEnter -= EdgeControl_MouseEnter;
                    break;
                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled) MouseLeave += EdgeControl_MouseLeave;
                    else MouseLeave -= EdgeControl_MouseLeave;
                    break;

                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled) MouseMove += EdgeControl_MouseMove;
                    else MouseMove -= EdgeControl_MouseMove;
                    break;
            }
            MouseUp -= EdgeControl_MouseUp;
            MouseUp += EdgeControl_MouseUp;
        }

        public EdgeControl()
            : this(null, null, null)
        {
        }

        public EdgeControl(VertexControl source, VertexControl target, object edge, bool showArrows = true)
        {
            DataContext = edge;
            Source = source; Target = target;
            Edge = edge; DataContext = edge;
            this.SetCurrentValue(ShowArrowsProperty, showArrows);
            IsHiddenEdgesUpdated = true;

            if (!this.IsInDesignMode())
            {
                EventOptions = new EdgeEventOptions(this);
                foreach (var item in Enum.GetValues(typeof (EventType)).Cast<EventType>())
                    UpdateEventhandling(item);

                // Trigger initial state
                SourceChanged();
                TargetChanged();
            }

            IsSelfLooped = IsSelfLoopedInternal;
        }

        #region Event handlers

        void EdgeControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_clickTrack)
                return;

            var curPoint = RootArea != null ? Mouse.GetPosition(RootArea) : new Point();

            if (curPoint != _clickTrackPoint)
                _clickTrack = false;
        }

        void EdgeControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                if (_clickTrack)
                {
                    RaiseEvent(new RoutedEventArgs(ClickEvent, this));
                    RootArea.OnEdgeClicked(this, e, Keyboard.Modifiers);
                }
            }
            _clickTrack = false;
            e.Handled = true;
        }

        void EdgeControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseLeave(this, null, Keyboard.Modifiers);
            // e.Handled = true;
        }

        void EdgeControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseEnter(this, null, Keyboard.Modifiers);
            // e.Handled = true;
        }

        void EdgeControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseMove(this, null, Keyboard.Modifiers);
            // e.Handled = true;
        }

        private void EdgeControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeDoubleClick(this, e, Keyboard.Modifiers);
            //e.Handled = true;
        }

        void EdgeControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeSelected(this, e, Keyboard.Modifiers);
            _clickTrack = true;
            _clickTrackPoint = RootArea != null ? Mouse.GetPosition(RootArea) : new Point();
            e.Handled = true;
        }

        #endregion

        #region Click Event
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EdgeControl));
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
        #endregion

        public override void Dispose()
        {
            Clean();
        }

        /// <summary>
        /// Gets Edge data as specified class
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        public T GetDataEdge<T>() where T : IGraphXCommonEdge
        {
            return (T)Edge;
        }   
    }
}