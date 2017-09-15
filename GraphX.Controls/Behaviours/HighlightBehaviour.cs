
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#endif
using GraphX.PCL.Common.Enums;

namespace GraphX.Controls
{
    public static class HighlightBehaviour
    {
        #region Attached props
        //trigger
        public static readonly DependencyProperty HighlightedProperty = DependencyProperty.RegisterAttached("Highlighted", typeof(bool), typeof(HighlightBehaviour), new PropertyMetadata(false));
        //settings
        public static readonly DependencyProperty IsHighlightEnabledProperty = DependencyProperty.RegisterAttached("IsHighlightEnabled", typeof(bool), typeof(HighlightBehaviour), new PropertyMetadata(false, OnIsHighlightEnabledPropertyChanged));
        public static readonly DependencyProperty HighlightControlProperty = DependencyProperty.RegisterAttached("HighlightControl", typeof(GraphControlType), typeof(HighlightBehaviour), new PropertyMetadata(GraphControlType.VertexAndEdge));
        public static readonly DependencyProperty HighlightEdgesProperty = DependencyProperty.RegisterAttached("HighlightEdges", typeof(EdgesType), typeof(HighlightBehaviour), new PropertyMetadata(EdgesType.Out));
        public static readonly DependencyProperty HighlightedEdgeTypeProperty = DependencyProperty.RegisterAttached("HighlightedEdgeType", typeof(HighlightedEdgeType), typeof(HighlightBehaviour), new PropertyMetadata(HighlightedEdgeType.None));

        public static HighlightedEdgeType GetHighlightedEdgeType(DependencyObject obj)
        {
            return (HighlightedEdgeType)obj.GetValue(HighlightedEdgeTypeProperty);
        }

        public static void SetHighlightedEdgeType(DependencyObject obj, HighlightedEdgeType value)
        {
            obj.SetValue(HighlightedEdgeTypeProperty, value);
        }

        public static bool GetIsHighlightEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHighlightEnabledProperty);
        }

        public static void SetIsHighlightEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHighlightEnabledProperty, value);
        }

        public static bool GetHighlighted(DependencyObject obj)
        {
            return (bool)obj.GetValue(HighlightedProperty);
        }

        public static void SetHighlighted(DependencyObject obj, bool value)
        {
            obj.SetValue(HighlightedProperty, value);
        }

        public static GraphControlType GetHighlightControl(DependencyObject obj)
        {
            return (GraphControlType)obj.GetValue(HighlightControlProperty);
        }

        public static void SetHighlightControl(DependencyObject obj, GraphControlType value)
        {
            obj.SetValue(HighlightControlProperty, value);
        }

        public static EdgesType GetHighlightEdges(DependencyObject obj)
        {
            return (EdgesType)obj.GetValue(HighlightEdgesProperty);
        }

        public static void SetHighlightEdges(DependencyObject obj, EdgesType value)
        {
            obj.SetValue(HighlightEdgesProperty, value);
        }

        #endregion

        #region PropertyChanged callbacks
        private static void OnIsHighlightEnabledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
#if WPF
            var element = obj as IInputElement;
#elif METRO
            var element = obj as FrameworkElement;
#endif
            if (element == null)
                    return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
            {
                //register the event handlers
#if WPF
                element.MouseEnter += element_MouseEnter;
                element.MouseLeave += element_MouseLeave;
#elif METRO
                element.PointerEntered += element_MouseEnter;
                element.PointerExited += element_MouseLeave;
#endif

            }
            else
            {
                //unregister the event handlers
#if WPF
                element.MouseEnter -= element_MouseEnter;
                element.MouseLeave -= element_MouseLeave;
#elif METRO
                element.PointerEntered -= element_MouseEnter;
                element.PointerExited -= element_MouseLeave;
#endif

            }
        }

#if WPF
        static void element_MouseLeave(object sender, MouseEventArgs e)
#elif METRO
        static void element_MouseLeave(object sender, PointerRoutedEventArgs e)
#endif
        {
            if (sender is DependencyObject == false) return;
            var ctrl = sender as IGraphControl;
            if (ctrl == null) return;

            var type = GetHighlightControl(sender as DependencyObject);
            var edgesType = GetHighlightEdges(sender as DependencyObject);
            SetHighlighted(sender as DependencyObject, false);

            if (type == GraphControlType.Vertex || type == GraphControlType.VertexAndEdge)
                foreach (var item in ctrl.RootArea.GetRelatedVertexControls(ctrl, edgesType).Cast<DependencyObject>())
                    SetHighlighted(item, false);

            if (type == GraphControlType.Edge || type == GraphControlType.VertexAndEdge)
                foreach (var item in ctrl.RootArea.GetRelatedEdgeControls(ctrl, edgesType).Cast<DependencyObject>())
                {
                    SetHighlighted(item, false);
                    SetHighlightedEdgeType(item, HighlightedEdgeType.None);
                }
        }

#if WPF
        static void element_MouseEnter(object sender, MouseEventArgs e)
#elif METRO
        static void element_MouseEnter(object sender, PointerRoutedEventArgs e)
#endif
        {
            if(sender is DependencyObject == false) return;
            var ctrl = sender as IGraphControl;
            if(ctrl == null) return;

            var type = GetHighlightControl(sender as DependencyObject);
            var edgesType = GetHighlightEdges(sender as DependencyObject);
            SetHighlighted(sender as DependencyObject, true);

            //highlight related vertices
            if(type == GraphControlType.Vertex || type == GraphControlType.VertexAndEdge)
                foreach (var item in ctrl.RootArea.GetRelatedVertexControls(ctrl, edgesType).Cast<DependencyObject>())
                    SetHighlighted(item, true);
            //highlight related edges
            if (type == GraphControlType.Edge || type == GraphControlType.VertexAndEdge)
            {
                //separetely get in and out edges to set direction flag
                if (edgesType == EdgesType.In || edgesType == EdgesType.All)
                    foreach (var item in ctrl.RootArea.GetRelatedEdgeControls(ctrl, EdgesType.In).Cast<DependencyObject>())
                    {
                        SetHighlighted(item, true);
                        SetHighlightedEdgeType(item, HighlightedEdgeType.In);
                    }
                if (edgesType == EdgesType.Out || edgesType == EdgesType.All)
                    foreach (var item in ctrl.RootArea.GetRelatedEdgeControls(ctrl, EdgesType.Out).Cast<DependencyObject>())
                    {
                        SetHighlighted(item, true);
                        SetHighlightedEdgeType(item, HighlightedEdgeType.Out);
                    }
            }
        }
        #endregion

        public enum HighlightType
        {
            Vertex,
            Edge,
            VertexAndEdge
        }
        
        public enum HighlightedEdgeType
        {
            In,
            Out,
            None
        }
    }
}