using GraphX;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ShowcaseExample.Models;
using GraphX.Controls;

namespace ShowcaseExample
{
    public partial class MainWindow
    {
        private void GeneralGraph_Constructor()
        {
            var gg_Logic = new LogicCoreExample();
            gg_Area.LogicCore = gg_Logic;

            //gg_Area.DefaulOverlapRemovalAlgorithm
            gg_layalgo.ItemsSource = Enum.GetValues(typeof(LayoutAlgorithmTypeEnum)).Cast<LayoutAlgorithmTypeEnum>();
            gg_layalgo.SelectedIndex = 0;
            gg_oralgo.ItemsSource = Enum.GetValues(typeof(OverlapRemovalAlgorithmTypeEnum)).Cast<OverlapRemovalAlgorithmTypeEnum>();
            gg_oralgo.SelectedIndex = 0;
            gg_eralgo.ItemsSource = Enum.GetValues(typeof(EdgeRoutingAlgorithmTypeEnum)).Cast<EdgeRoutingAlgorithmTypeEnum>();
            gg_eralgo.SelectedIndex = 2;
            gg_but_randomgraph.Click += gg_but_randomgraph_Click;
            gg_vertexCount.Text = "30";
            gg_async.Checked += gg_async_Checked;
            gg_async.Unchecked += gg_async_Checked;
            gg_Area.RelayoutFinished += gg_Area_RelayoutFinished;
            gg_Area.GenerateGraphFinished += gg_Area_GenerateGraphFinished;
            gg_Logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;

            ZoomControl.SetViewFinderVisibility(gg_zoomctrl, System.Windows.Visibility.Visible);

            gg_zoomctrl.IsAnimationDisabled = false;
            gg_zoomctrl.MaxZoomDelta = 2;

            this.Loaded += GG_Loaded;
        }

        void GG_Loaded(object sender, RoutedEventArgs e)
        {
            GG_RegisterCommands();
        }

        #region Commands

        #region GGRelayoutCommand

        private bool GGRelayoutCommandCanExecute(object sender)
        {
            return true;
        }

        private void GGRelayoutCommandExecute(object sender)
        {
            if (gg_Area.LogicCore.AsyncAlgorithmCompute)
                gg_loader.Visibility = System.Windows.Visibility.Visible;
            gg_Area.RelayoutGraph(true);
        }
        #endregion

        #region SaveStateCommand
        public static RoutedCommand SaveStateCommand = new RoutedCommand();
        private void SaveStateCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = gg_Area.LogicCore.Graph != null && gg_Area.VertexList.Count > 0;
        }

        private void SaveStateCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (gg_Area.StateStorage.ContainsState("exampleState"))
                gg_Area.StateStorage.RemoveState("exampleState");
            gg_Area.StateStorage.SaveState("exampleState", "My example state");
        }
        #endregion

        #region LoadStateCommand
        public static RoutedCommand LoadStateCommand = new RoutedCommand();
        private void LoadStateCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = gg_Area.StateStorage.ContainsState("exampleState");
        }

        private void LoadStateCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (gg_Area.StateStorage.ContainsState("exampleState"))
                gg_Area.StateStorage.LoadState("exampleState");
        }
        #endregion

        #region SaveLayoutCommand
        public static RoutedCommand SaveLayoutCommand = new RoutedCommand();
        private void SaveLayoutCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = gg_Area.LogicCore.Graph != null && gg_Area.VertexList.Count > 0;
        }

        private void SaveLayoutCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new SaveFileDialog() { Filter = "All files|*.*", Title = "Select layout file name", FileName = "laytest.xml" };
            if (dlg.ShowDialog() == true)
            {
                gg_Area.SaveIntoFile(dlg.FileName);
            }
        }
        #endregion

        #region LoadLayoutCommand

        #region Save / load visual graph example

        /// <summary>
        /// Temporary storage for example vertex data objects used on save/load mechanics
        /// </summary>
        private Dictionary<int, DataVertex> exampleVertexStorage = new Dictionary<int, DataVertex>();
        public DataVertex gg_getVertex(int id)
        {
            var item = DataSource.FirstOrDefault(a => a.ID == id);
            if (item == null) item = new Models.DataItem() { ID = id, Text = id.ToString() };
            exampleVertexStorage.Add(id, new DataVertex() { ID = item.ID, Text = item.Text, DataImage = new BitmapImage(new Uri(@"pack://application:,,,/GraphX.Controls;component/Images/help_black.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad } });
            return exampleVertexStorage.Last().Value;
        }

        public DataEdge gg_getEdge(int ids, int idt)
        {
            return new DataEdge(exampleVertexStorage.ContainsKey(ids) ? exampleVertexStorage[ids] : null,
                exampleVertexStorage.ContainsKey(idt) ? exampleVertexStorage[idt] : null);
        }

        #endregion

        public static RoutedCommand LoadLayoutCommand = new RoutedCommand();
        private void LoadLayoutCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void LoadLayoutCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog() { Filter = "All files|*.*", Title = "Select layout file", FileName = "laytest.xml" };
            if (dlg.ShowDialog() == true)
            {
                exampleVertexStorage.Clear();
                try
                {
                    gg_Area.LoadFromFile(dlg.FileName);
                    gg_Area.SetVerticesDrag(true);
                    gg_Area.UpdateAllEdges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Failed to load layout file:\n {0}", ex.ToString()));
                }
            }
        }
        #endregion

        void GG_RegisterCommands()
        {
            CommandBindings.Add(new CommandBinding(SaveStateCommand, SaveStateCommandExecute, SaveStateCommandCanExecute));
            gg_saveState.Command = SaveStateCommand;
            CommandBindings.Add(new CommandBinding(LoadStateCommand, LoadStateCommandExecute, LoadStateCommandCanExecute));
            gg_loadState.Command = LoadStateCommand;

            CommandBindings.Add(new CommandBinding(SaveLayoutCommand, SaveLayoutCommandExecute, SaveLayoutCommandCanExecute));
            gg_saveLayout.Command = SaveLayoutCommand;
            CommandBindings.Add(new CommandBinding(LoadLayoutCommand, LoadLayoutCommandExecute, LoadLayoutCommandCanExecute));
            gg_loadLayout.Command = LoadLayoutCommand;

            gg_but_relayout.Command = new SimpleCommand(GGRelayoutCommandCanExecute, GGRelayoutCommandExecute);
        }


        #endregion

        void gg_async_Checked(object sender, RoutedEventArgs e)
        {
            gg_Area.LogicCore.AsyncAlgorithmCompute = (bool)gg_async.IsChecked;
        }

        private void gg_saveAsPngImage_Click(object sender, RoutedEventArgs e)
        {
            gg_Area.ExportAsImage(ImageType.PNG);
        }

        private void gg_printlay_Click(object sender, RoutedEventArgs e)
        {
            gg_Area.PrintDialog("GraphX layout printing");
        }

        private void gg_vertexCount_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = CustomHelper.IsIntegerInput(e.Text) && Convert.ToInt32(e.Text) <= datasourceSize;
        }

        private void gg_layalgo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            gg_Area.GetLogicCore<LogicCoreExample>().DefaultLayoutAlgorithm = (LayoutAlgorithmTypeEnum)gg_layalgo.SelectedItem;
            if (gg_Area.LogicCore.Graph == null) gg_but_randomgraph_Click(null, null);
            else gg_Area.RelayoutGraph();
            gg_Area.GenerateAllEdges();
        }

        private void gg_useExternalLayAlgo_Checked(object sender, RoutedEventArgs e)
        {
            if (gg_useExternalLayAlgo.IsChecked == true)
            {
                var graph = gg_Area.LogicCore.Graph == null ? GenerateDataGraph(Convert.ToInt32(gg_vertexCount.Text)) : gg_Area.LogicCore.Graph;
                gg_Area.LogicCore.Graph = graph;
                AssignExternalLayoutAlgorithm(graph);
            }
            else gg_Area.GetLogicCore<LogicCoreExample>().ExternalLayoutAlgorithm = null;
        }

        private void AssignExternalLayoutAlgorithm(BidirectionalGraph<DataVertex, DataEdge> graph)
        {
            gg_Area.GetLogicCore<LogicCoreExample>().ExternalLayoutAlgorithm = gg_Area.LogicCore.AlgorithmFactory.CreateLayoutAlgorithm(LayoutAlgorithmTypeEnum.ISOM, graph, null, null, null);
        }

        private void gg_oralgo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            gg_Area.GetLogicCore<LogicCoreExample>().DefaultOverlapRemovalAlgorithm = (OverlapRemovalAlgorithmTypeEnum)gg_oralgo.SelectedItem;
            if (gg_Area.LogicCore.Graph == null) gg_but_randomgraph_Click(null, null);
            else gg_Area.RelayoutGraph();
            gg_Area.GenerateAllEdges();
        }

        private void gg_useExternalORAlgo_Checked(object sender, RoutedEventArgs e)
        {
            if (gg_useExternalORAlgo.IsChecked == true)
            {
                gg_Area.GetLogicCore<LogicCoreExample>().ExternalOverlapRemovalAlgorithm = gg_Area.LogicCore.AlgorithmFactory.CreateOverlapRemovalAlgorithm(OverlapRemovalAlgorithmTypeEnum.FSA, null, null);
            }
            else gg_Area.GetLogicCore<LogicCoreExample>().ExternalOverlapRemovalAlgorithm = null;
        }

        private void gg_eralgo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            gg_Area.GetLogicCore<LogicCoreExample>().DefaultEdgeRoutingAlgorithm = (EdgeRoutingAlgorithmTypeEnum)gg_eralgo.SelectedItem;
            if (gg_Area.LogicCore.Graph == null) gg_but_randomgraph_Click(null, null);
            else gg_Area.RelayoutGraph();
            gg_Area.GenerateAllEdges();
        }

        private void gg_useExternalERAlgo_Checked(object sender, RoutedEventArgs e)
        {
            if (gg_useExternalERAlgo.IsChecked == true)
            {
                var graph = gg_Area.LogicCore.Graph ?? GenerateDataGraph(Convert.ToInt32(gg_vertexCount.Text));
                gg_Area.LogicCore.Graph = graph;
                gg_Area.GetLogicCore<LogicCoreExample>().ExternalEdgeRoutingAlgorithm = gg_Area.LogicCore.AlgorithmFactory.CreateEdgeRoutingAlgorithm(EdgeRoutingAlgorithmTypeEnum.SimpleER, new GraphX.Measure.Rect(gg_Area.DesiredSize.ToGraphX()), graph, null, null, null);
            }
            else gg_Area.GetLogicCore<LogicCoreExample>().ExternalEdgeRoutingAlgorithm = null;
        }

        void gg_Area_RelayoutFinished(object sender, EventArgs e)
        {
            if (gg_Area.LogicCore.AsyncAlgorithmCompute)
                gg_loader.Visibility = System.Windows.Visibility.Collapsed;
            gg_zoomctrl.ZoomToFill();
            gg_zoomctrl.Mode = ZoomControlModes.Custom;
        }

        /// <summary>
        /// Use this event in case we have chosen async computation
        /// </summary>
        void gg_Area_GenerateGraphFinished(object sender, EventArgs e)
        {

            gg_Area.GenerateAllEdges();
            if (gg_Area.LogicCore.AsyncAlgorithmCompute)
                gg_loader.Visibility = System.Windows.Visibility.Collapsed;

            gg_zoomctrl.ZoomToFill();
            gg_zoomctrl.Mode = ZoomControlModes.Custom;
        }

        private void gg_but_randomgraph_Click(object sender, RoutedEventArgs e)
        {
            gg_Area.ClearLayout();
            var graph = GenerateDataGraph(Convert.ToInt32(gg_vertexCount.Text));
            //assign graph again as we need to update Graph param inside and i have no independent examples
            if (gg_Area.GetLogicCore<LogicCoreExample>().ExternalLayoutAlgorithm != null)
                AssignExternalLayoutAlgorithm(graph);

            //supplied graph will be automaticaly be assigned to GraphArea::LogicCore.Graph property
            gg_Area.GenerateGraph(graph);
            gg_Area.SetVerticesDrag(true);

            if (gg_Area.LogicCore.AsyncAlgorithmCompute)
                gg_loader.Visibility = System.Windows.Visibility.Visible;
        }

    }
}
