using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;

namespace GraphX.Controls
{
	/// <summary>
	/// Visual vertex control
	/// </summary>
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(IVertexLabelControl))]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Pressed")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerLeave")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    public class VertexControl : VertexControlBase
    {

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

            IsEnabledChanged += VertexControl_IsEnabledChanged;

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

        void VertexControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null) return;
            VertexLabelControl = this.FindDescendantByName("PART_vertexLabel") as IVertexLabelControl;
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

        void VertexControl_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseLeave(this, e);
            VisualStateManager.GoToState(this, "PointerLeave", true);

            //e.Handled = true;
        }

        void VertexControl_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnVertexMouseEnter(this, e);
            }
            VisualStateManager.GoToState(this, "PointerOver", true);

            // e.Handled = true;
        }

        void VertexControl_MouseMove(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null)
                RootArea.OnVertexMouseMove(this, e);

        }

        void VertexControl_Down(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexSelected(this, e);
            //e.Handled = true;
            VisualStateManager.GoToState(this, "Pressed", true);
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
                EventOptions.Clean();
            }
        }

    }
}