using GraphX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GraphX.Models;
using System.Windows.Input;
using ShowcaseExample.Models;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using System.Windows.Media.Imaging;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Markup;
using GraphX.Controls;

namespace ShowcaseExample
{
    public partial class MainWindow
    {
        
        private void ThemedGraph_Constructor()
        {
            var tg_Logic = new LogicCoreExample();
            tg_Area.LogicCore = tg_Logic;
            tg_Logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
            tg_Logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            tg_Logic.DefaultOverlapRemovalAlgorithmParams = tg_Logic.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            (tg_Logic.DefaultOverlapRemovalAlgorithmParams as OverlapRemovalParameters).HorizontalGap = 150;
            (tg_Logic.DefaultOverlapRemovalAlgorithmParams as OverlapRemovalParameters).VerticalGap = 150;
            tg_Logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            tg_Logic.EdgeCurvingEnabled = true;
            tg_Logic.AsyncAlgorithmCompute = true;

            tg_edgeMode.ItemsSource = new string[] { "Draw all edges", "Draw edges by vertex selection" };
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
            //tg_Area.UseNativeObjectArrange = false;
            //tg_Area.SideExpansionSize = new Size(100, 100); //additional space for correct scale animation

            //tg_Area.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;

            tg_highlightEnabled_Checked(null, null);
            tg_dragMoveEdges_Checked(null, null);
            tg_dragEnabled_Checked(null, null);


            tg_Area.VertexSelected += tg_Area_VertexSelected;
            tg_Area.GenerateGraphFinished += tg_Area_GenerateGraphFinished;
            tg_Area.RelayoutFinished += tg_Area_RelayoutFinished;
            tg_dragMoveEdges.Checked += tg_dragMoveEdges_Checked;
            tg_dragMoveEdges.Unchecked += tg_dragMoveEdges_Checked;

            ZoomControl.SetViewFinderVisibility(tg_zoomctrl, System.Windows.Visibility.Visible);

            TGRegisterCommands();
        }


        #region Commands

        #region TGRelayoutCommand
        private bool TGRelayoutCommandCanExecute(object sender)
        {
            return true; // tg_Area.Graph != null && tg_Area.VertexList.Count > 0;
        }

        private void TGRelayoutCommandExecute(object sender)
        {
            if (tg_Area.LogicCore.AsyncAlgorithmCompute)
                tg_loader.Visibility = System.Windows.Visibility.Visible;

            tg_Area.RelayoutGraph(true);
            /*if (tg_edgeMode.SelectedIndex == 0 && tg_Area.EdgesList.Count == 0)
                tg_Area.GenerateAllEdges();*/

        }
        #endregion

        void TGRegisterCommands()
        {
            tg_but_relayout.Command = new SimpleCommand(TGRelayoutCommandCanExecute, TGRelayoutCommandExecute);
        }
        #endregion

        void tg_Area_VertexSelected(object sender, VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton ==  MouseButtonState.Pressed && tg_edgeMode.SelectedIndex == 1)
            {
                tg_Area.GenerateEdgesForVertex(args.VertexControl, (EdgesType)tg_edgeType.SelectedItem);                
            }
            if (args.MouseArgs.RightButton == MouseButtonState.Pressed)
            {
                args.VertexControl.ContextMenu = new System.Windows.Controls.ContextMenu();
                var menuitem = new System.Windows.Controls.MenuItem() { Header = "Delete item", Tag = args.VertexControl };
                menuitem.Click += tg_deleteitem_Click;
                args.VertexControl.ContextMenu.Items.Add(menuitem);
                
                var str = new StringBuilder();
                using (var writer = new StringWriter(str))
                    XamlWriter.Save(args.VertexControl.ContextMenu.Template, writer);
                Debug.Write(str);
            }
        }

        void tg_deleteitem_Click(object sender, RoutedEventArgs e)
        {
            var vc = (sender as System.Windows.Controls.MenuItem).Tag as VertexControl;
            if (vc != null)
            {
                foreach (var item in tg_Area.GetRelatedControls(vc, GraphControlType.Edge, EdgesType.All))
                {
                    var ec = item as EdgeControl;
                    tg_Area.LogicCore.Graph.RemoveEdge(ec.Edge as DataEdge);
                    tg_Area.RemoveEdge(ec.Edge as DataEdge);
                }
                tg_Area.RemoveVertex(vc.Vertex as DataVertex);
                tg_Area.LogicCore.Graph.RemoveVertex(vc.Vertex as DataVertex);
            } 
        }

        private void tg_but_randomgraph_Click(object sender, RoutedEventArgs e)
        {
            var graph = GenerateDataGraph(Rand.Next(10, 20));

            foreach (var item in graph.Vertices)
            {
                item.Age = Rand.Next(18, 75);
                item.Gender = ThemedDataStorage.Gender[Rand.Next(0, 2)];
                if (item.Gender == ThemedDataStorage.Gender[0])
                    item.PersonImage = new BitmapImage(new Uri(@"pack://application:,,,/ShowcaseExample;component/Images/female.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad };
                else item.PersonImage = new BitmapImage(new Uri(@"pack://application:,,,/ShowcaseExample;component/Images/male.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad };
                item.Profession = ThemedDataStorage.Professions[Rand.Next(0, ThemedDataStorage.Professions.Count -1)];
                item.Name = ThemedDataStorage.Names[Rand.Next(0, ThemedDataStorage.Names.Count -1)];
            }
            foreach (var item in graph.Edges)
            {
                item.ToolTipText = string.Format("{0} -> {1}", item.Source.Name, item.Target.Name);
            }

            //TIP: trick to disable zoomcontrol behaviour when it is performing fill animation from top left zoomed corner
            //instead we will fill-animate from maximum zoom distance            
            //tg_zoomctrl.Zoom = 0.01; //disable zoom control auto fill animation by setting this value
            tg_Area.GenerateGraph(graph, tg_edgeMode.SelectedIndex == 0 ? true : false);

            if (tg_Area.LogicCore.AsyncAlgorithmCompute)
                tg_loader.Visibility = System.Windows.Visibility.Visible;
        }

        void tg_Area_RelayoutFinished(object sender, EventArgs e)
        {
            if (tg_Area.LogicCore.AsyncAlgorithmCompute)
                tg_loader.Visibility = System.Windows.Visibility.Collapsed;

            tg_zoomctrl.ZoomToFill();
        }

        void tg_Area_GenerateGraphFinished(object sender, EventArgs e)
        {
            if (tg_Area.LogicCore.AsyncAlgorithmCompute)
                tg_loader.Visibility = System.Windows.Visibility.Collapsed;


            tg_highlightStrategy_SelectionChanged(null, null);
            tg_highlightType_SelectionChanged(null, null);
            tg_highlightEnabled_Checked(null, null);
            tg_highlightEdgeType_SelectionChanged(null, null);
            tg_dragMoveEdges_Checked(null, null);
            tg_dragEnabled_Checked(null, null);

            tg_Area.SetEdgesDashStyle(EdgeDashStyle.Dash);
            tg_zoomctrl.ZoomToFill();// ZoomToFill(); //manually update zoom control to fill the area
        }

        private void tg_edgeMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void tg_edgeType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        void tg_dragMoveEdges_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                DragBehaviour.SetUpdateEdgesOnMove(item.Value, (bool)tg_dragMoveEdges.IsChecked);
        }


        private void tg_dragEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (tg_dragEnabled.IsChecked == true) tg_dragMoveEdges.Visibility = System.Windows.Visibility.Visible;
            else tg_dragMoveEdges.Visibility = System.Windows.Visibility.Collapsed;

            foreach (var item in tg_Area.VertexList)
            {
                DragBehaviour.SetIsDragEnabled(item.Value, (bool)tg_dragEnabled.IsChecked);
                DragBehaviour.SetUpdateEdgesOnMove(item.Value, true);
            }
        }

        private void tg_moveAnimation_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch ((MoveAnimation)tg_moveAnimation.SelectedItem)
            {
                case MoveAnimation.None: 
                    tg_Area.MoveAnimation = null;
                    break;
                default:
                    tg_Area.MoveAnimation = AnimationFactory.CreateMoveAnimation((MoveAnimation)tg_moveAnimation.SelectedItem, TimeSpan.FromSeconds(1));
                    break;
            }
        }

        private void tg_deleteAnimation_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

        private void tg_mouseoverAnimation_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

        private void tg_highlightStrategy_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                HighlightBehaviour.SetHighlightStrategy(item.Value, (HighlightStrategy)tg_highlightStrategy.SelectedItem);
            foreach (var item in tg_Area.EdgesList)
                HighlightBehaviour.SetHighlightStrategy(item.Value, (HighlightStrategy)tg_highlightStrategy.SelectedItem);
        }

        private void tg_highlightType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                HighlightBehaviour.SetHighlightControl(item.Value, (GraphControlType)tg_highlightType.SelectedItem);
            foreach (var item in tg_Area.EdgesList)
                HighlightBehaviour.SetHighlightControl(item.Value, (GraphControlType)tg_highlightType.SelectedItem);
        }

        private void tg_highlightEnabled_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, (bool)tg_highlightEnabled.IsChecked);
            foreach (var item in tg_Area.EdgesList)
                HighlightBehaviour.SetIsHighlightEnabled(item.Value, (bool)tg_highlightEnabled.IsChecked);

            tg_highlightStrategy.IsEnabled = tg_highlightType.IsEnabled = tg_highlightEdgeType.IsEnabled = (bool)tg_highlightEnabled.IsChecked;
        }

        private void tg_highlightEdgeType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (var item in tg_Area.VertexList)
                HighlightBehaviour.SetHighlightEdges(item.Value, (EdgesType)tg_highlightEdgeType.SelectedItem);
            foreach (var item in tg_Area.EdgesList)
                HighlightBehaviour.SetHighlightEdges(item.Value, (EdgesType)tg_highlightEdgeType.SelectedItem);
        }

    }
}
