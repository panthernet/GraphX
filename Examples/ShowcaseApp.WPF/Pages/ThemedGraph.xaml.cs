using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphX;
using GraphX.PCL.Common.Enums;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for ThemedGraph.xaml
    /// </summary>
    public partial class ThemedGraph : INotifyPropertyChanged
    {
       // private ZoomControl tg_zoomctrl = new ZoomControl();

        public ThemedGraph()
        {
            InitializeComponent();

            var logic = new LogicCoreExample();
            tg_Area.LogicCore = logic;
            logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logic.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
            logic.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;
            logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            logic.EdgeCurvingEnabled = true;
            logic.AsyncAlgorithmCompute = true;
            tg_Area.SetVerticesDrag(true);
            tg_dragEnabled.IsChecked = true;

            tg_edgeMode.ItemsSource = new[] { "Draw all", "Draw for selected" };
            tg_edgeMode.SelectedIndex = 0;
            tg_edgeType.ItemsSource = Enum.GetValues(typeof(EdgesType)).Cast<EdgesType>();
            tg_edgeType.SelectedItem = EdgesType.All;
            tg_moveAnimation.ItemsSource = Enum.GetValues(typeof(MoveAnimation)).Cast<MoveAnimation>();
            tg_moveAnimation.SelectedItem = MoveAnimation.Move;
            tg_deleteAnimation.ItemsSource = Enum.GetValues(typeof(DeleteAnimation)).Cast<DeleteAnimation>();
            tg_deleteAnimation.SelectedItem = DeleteAnimation.Shrink;
            tg_mouseoverAnimation.ItemsSource = Enum.GetValues(typeof(MouseOverAnimation)).Cast<MouseOverAnimation>();
            tg_mouseoverAnimation.SelectedItem = MouseOverAnimation.Scale;
            tg_highlightStrategy.ItemsSource = Enum.GetValues(typeof(HighlightStrategy)).Cast<HighlightStrategy>();
            tg_highlightStrategy.SelectedItem = HighlightStrategy.UseExistingControls;
            tg_highlightType.ItemsSource = Enum.GetValues(typeof(GraphControlType)).Cast<GraphControlType>();
            tg_highlightType.SelectedItem = GraphControlType.VertexAndEdge;
            tg_highlightEdgeType.ItemsSource = Enum.GetValues(typeof(EdgesType)).Cast<EdgesType>();
            tg_highlightEdgeType.SelectedItem = EdgesType.All;
            tg_highlightEnabled_Checked(null, null);
            tg_dragMoveEdges_Checked(null, null);
            tg_dragEnabled_Checked(null, null);


            tg_Area.VertexSelected += tg_Area_VertexSelected;
            tg_Area.GenerateGraphFinished += tg_Area_GenerateGraphFinished;
            tg_Area.RelayoutFinished += tg_Area_RelayoutFinished;
            tg_dragMoveEdges.Checked += tg_dragMoveEdges_Checked;
            tg_dragMoveEdges.Unchecked += tg_dragMoveEdges_Checked;

            ZoomControl.SetViewFinderVisibility(tg_zoomctrl, Visibility.Visible);

            TgRegisterCommands();               
        }

        #region Commands

        #region TGRelayoutCommand
        private static bool TgRelayoutCommandCanExecute(object sender)
        {
            return true;
        }

        private void TgRelayoutCommandExecute(object sender)
        {
            if (tg_Area.LogicCore.AsyncAlgorithmCompute)
                tg_loader.Visibility = Visibility.Visible;

            tg_Area.RelayoutGraph(true);
        }
        #endregion

        void TgRegisterCommands()
        {
            tg_but_relayout.Command = new SimpleCommand(TgRelayoutCommandCanExecute, TgRelayoutCommandExecute);
        }
        #endregion

        void tg_Area_VertexSelected(object sender, VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton == MouseButtonState.Pressed && tg_edgeMode.SelectedIndex == 1)
            {
                tg_Area.GenerateEdgesForVertex(args.VertexControl, (EdgesType)tg_edgeType.SelectedItem);
            }
            if (args.MouseArgs.RightButton != MouseButtonState.Pressed) return;
            args.VertexControl.ContextMenu = new ContextMenu();
            var menuitem = new MenuItem { Header = "Delete item", Tag = args.VertexControl };
            menuitem.Click += tg_deleteitem_Click;
            args.VertexControl.ContextMenu.Items.Add(menuitem);
            args.VertexControl.ContextMenu.IsOpen = true;
        }

        void tg_deleteitem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var vc = menuItem.Tag as VertexControl;
            if (vc == null) return;
            foreach (var ec in tg_Area.GetRelatedControls(vc, GraphControlType.Edge, EdgesType.All).Select(item => item as EdgeControl)) {
                tg_Area.LogicCore.Graph.RemoveEdge(ec.Edge as DataEdge);
                tg_Area.RemoveEdge(ec.Edge as DataEdge);
            }
            tg_Area.RemoveVertex(vc.Vertex as DataVertex);
            tg_Area.LogicCore.Graph.RemoveVertex(vc.Vertex as DataVertex);
        }

        private void tg_but_randomgraph_Click(object sender, RoutedEventArgs e)
        {
            var graph = ShowcaseHelper.GenerateDataGraph(ShowcaseHelper.Rand.Next(10, 20));

            foreach (var item in graph.Vertices)
            {
                ThemedDataStorage.FillDataVertex(item);
            }
            foreach (var item in graph.Edges)
            {
                item.ToolTipText = string.Format("{0} -> {1}", item.Source.Name, item.Target.Name);
            }

            //TIP: trick to disable zoomcontrol behaviour when it is performing fill animation from top left zoomed corner
            //instead we will fill-animate from maximum zoom distance            
            //tg_zoomctrl.Zoom = 0.01; //disable zoom control auto fill animation by setting this value
            tg_Area.GenerateGraph(graph, tg_edgeMode.SelectedIndex == 0);

            if (tg_Area.LogicCore.AsyncAlgorithmCompute)
                tg_loader.Visibility = Visibility.Visible;
        }

        void tg_Area_RelayoutFinished(object sender, EventArgs e)
        {
            if (tg_Area.LogicCore.AsyncAlgorithmCompute)
                tg_loader.Visibility = Visibility.Collapsed;
            if(tg_Area.MoveAnimation == null)
             tg_zoomctrl.ZoomToFill();
        }

        void tg_Area_GenerateGraphFinished(object sender, EventArgs e)
        {
            if (tg_Area.LogicCore.AsyncAlgorithmCompute)
                tg_loader.Visibility = Visibility.Collapsed;


            tg_highlightStrategy_SelectionChanged(null, null);
            tg_highlightType_SelectionChanged(null, null);
            tg_highlightEnabled_Checked(null, null);
            tg_highlightEdgeType_SelectionChanged(null, null);
            tg_dragMoveEdges_Checked(null, null);
            tg_dragEnabled_Checked(null, null);

            tg_Area.SetEdgesDashStyle(EdgeDashStyle.Dash);
            tg_zoomctrl.ZoomToFill();// ZoomToFill(); //manually update zoom control to fill the area
        }

        private void tg_edgeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void tg_edgeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        void tg_dragMoveEdges_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                DragBehaviour.SetUpdateEdgesOnMove(item.Value, tg_dragMoveEdges.IsChecked != null && tg_dragMoveEdges.IsChecked.Value);
        }


        private void tg_dragEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (tg_dragEnabled.IsChecked == null) return;
            tg_dragMoveEdges.IsEnabled = (bool)tg_dragEnabled.IsChecked;

            foreach (var item in tg_Area.VertexList)
            {
                DragBehaviour.SetIsDragEnabled(item.Value, tg_dragEnabled.IsChecked != null && tg_dragEnabled.IsChecked.Value);
                DragBehaviour.SetUpdateEdgesOnMove(item.Value, true);
            }
        }

        private void tg_moveAnimation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((MoveAnimation)tg_moveAnimation.SelectedItem)
            {
                case MoveAnimation.None:
                    tg_Area.MoveAnimation = null;
                    break;
                default:
                    tg_Area.MoveAnimation = AnimationFactory.CreateMoveAnimation((MoveAnimation)tg_moveAnimation.SelectedItem, TimeSpan.FromSeconds(1));
                    tg_Area.MoveAnimation.Completed += MoveAnimation_Completed;
                    break;
            }
        }

        void MoveAnimation_Completed(object sender, EventArgs e)
        {
            tg_zoomctrl.ZoomToFill();
        }

        private void tg_deleteAnimation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((DeleteAnimation)tg_deleteAnimation.SelectedItem)
            {
                case DeleteAnimation.None:
                    tg_Area.DeleteAnimation = null;
                    break;
                case DeleteAnimation.Shrink:
                    tg_Area.DeleteAnimation = AnimationFactory.CreateDeleteAnimation((DeleteAnimation)tg_deleteAnimation.SelectedItem);
                    break;
                default:
                    tg_Area.DeleteAnimation = AnimationFactory.CreateDeleteAnimation((DeleteAnimation)tg_deleteAnimation.SelectedItem);
                    break;
            }
        }

        private void tg_mouseoverAnimation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((MouseOverAnimation)tg_mouseoverAnimation.SelectedItem)
            {
                case MouseOverAnimation.None:
                    tg_Area.MouseOverAnimation = null;
                    break;
                default:
                    tg_Area.MouseOverAnimation = AnimationFactory.CreateMouseOverAnimation((MouseOverAnimation)tg_mouseoverAnimation.SelectedItem);
                    break;
            }
        }

        private void tg_highlightStrategy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                HighlightBehaviour.SetHighlightStrategy(item.Value, (HighlightStrategy)tg_highlightStrategy.SelectedItem);
            foreach (var item in tg_Area.EdgesList)
                HighlightBehaviour.SetHighlightStrategy(item.Value, (HighlightStrategy)tg_highlightStrategy.SelectedItem);
        }

        private void tg_highlightType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                HighlightBehaviour.SetHighlightControl(item.Value, (GraphControlType)tg_highlightType.SelectedItem);
            foreach (var item in tg_Area.EdgesList)
                HighlightBehaviour.SetHighlightControl(item.Value, (GraphControlType)tg_highlightType.SelectedItem);
        }

        private void tg_highlightEnabled_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, tg_highlightEnabled.IsChecked != null && tg_highlightEnabled.IsChecked.Value);
            foreach (var item in tg_Area.EdgesList)
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, tg_highlightEnabled.IsChecked != null && tg_highlightEnabled.IsChecked.Value);

            tg_highlightStrategy.IsEnabled = tg_highlightType.IsEnabled = tg_highlightEdgeType.IsEnabled = tg_highlightEnabled.IsChecked != null && tg_highlightEnabled.IsChecked.Value;
        }

        private void tg_highlightEdgeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                HighlightBehaviour.SetHighlightEdges(item.Value, (EdgesType)tg_highlightEdgeType.SelectedItem);
            foreach (var item in tg_Area.EdgesList)
                HighlightBehaviour.SetHighlightEdges(item.Value, (EdgesType)tg_highlightEdgeType.SelectedItem);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
