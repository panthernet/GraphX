using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.Controls.Models;

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

        #region Position trace feature



        private DependencyPropertyDescriptor _sxDescriptor;
        private DependencyPropertyDescriptor _syDescriptor;
        internal void UpdatePositionTraceState()
        {
            if (EventOptions.PositionChangeNotification)
            {
                _sxDescriptor = DependencyPropertyDescriptor.FromProperty(GraphAreaBase.XProperty, typeof(VertexControl));
                _sxDescriptor.AddValueChanged(this, source_PositionChanged);
                _syDescriptor = DependencyPropertyDescriptor.FromProperty(GraphAreaBase.YProperty, typeof(VertexControl));
                _syDescriptor.AddValueChanged(this, source_PositionChanged);
            }
            else
            {
                if (_sxDescriptor != null)
                    _sxDescriptor.RemoveValueChanged(this, source_PositionChanged);
                if (_syDescriptor != null)
                    _syDescriptor.RemoveValueChanged(this, source_PositionChanged);
            }
        }

        private void source_PositionChanged(object sender, EventArgs e)
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

        internal void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled) MouseDown += VertexControl_Down;
                    else MouseDown -= VertexControl_Down;
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

        void VertexControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseUp(this, e, Keyboard.Modifiers);
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
            e.Handled = true;
        }

        void VertexControl_Down(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexSelected(this, e, Keyboard.Modifiers);
            e.Handled = true;
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


    }
}