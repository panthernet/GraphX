using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphX.PCL.Common.Enums;
using GraphX.Controls;
using Microsoft.Win32;
using QuickGraph;
using ShowcaseApp.WPF.FileSerialization;
using ShowcaseApp.WPF.Models;
using Rect = GraphX.Measure.Rect;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for GeneralGraph.xaml
    /// </summary>
    public partial class GeneralGraph : UserControl
    {
        public GeneralGraph()
        {
            InitializeComponent();
            DataContext = this;

            gg_vertexCount.Text = "30";
            var ggLogic = new LogicCoreExample();
            gg_Area.LogicCore = ggLogic;

            gg_layalgo.SelectionChanged += gg_layalgo_SelectionChanged;
            gg_oralgo.SelectionChanged += gg_oralgo_SelectionChanged;
            gg_eralgo.SelectionChanged += gg_eralgo_SelectionChanged;

            gg_layalgo.ItemsSource = Enum.GetValues(typeof(LayoutAlgorithmTypeEnum)).Cast<LayoutAlgorithmTypeEnum>();
            gg_layalgo.SelectedItem = LayoutAlgorithmTypeEnum.KK;

            gg_oralgo.ItemsSource = Enum.GetValues(typeof(OverlapRemovalAlgorithmTypeEnum)).Cast<OverlapRemovalAlgorithmTypeEnum>();
            gg_oralgo.SelectedIndex = 0;

            gg_eralgo.ItemsSource = Enum.GetValues(typeof(EdgeRoutingAlgorithmTypeEnum)).Cast<EdgeRoutingAlgorithmTypeEnum>();
            gg_eralgo.SelectedItem = EdgeRoutingAlgorithmTypeEnum.SimpleER;

            gg_but_randomgraph.Click += gg_but_randomgraph_Click;
            gg_async.Checked += gg_async_Checked;
            gg_async.Unchecked += gg_async_Checked;
            gg_Area.RelayoutFinished += gg_Area_RelayoutFinished;
            gg_Area.GenerateGraphFinished += gg_Area_GenerateGraphFinished;

            ggLogic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            ggLogic.EdgeCurvingEnabled = true;                  
            gg_Area.ShowAllEdgesArrows(true);

            ZoomControl.SetViewFinderVisibility(gg_zoomctrl, Visibility.Visible);

            gg_zoomctrl.IsAnimationDisabled = false;
            gg_zoomctrl.MaximumZoomStep = 2;

            Loaded += GG_Loaded;
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

        private void GgRelayoutCommandExecute(object sender)
        {
            if (gg_Area.LogicCore.AsyncAlgorithmCompute)
                gg_loader.Visibility = Visibility.Visible;
            gg_Area.RelayoutGraph(true);
        }
        #endregion

        #region SaveStateCommand
        private static readonly RoutedCommand SaveStateCommand = new RoutedCommand();
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
        private static readonly RoutedCommand LoadStateCommand = new RoutedCommand();
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
        private static readonly RoutedCommand SaveLayoutCommand = new RoutedCommand();
        private void SaveLayoutCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = gg_Area.LogicCore.Graph != null && gg_Area.VertexList.Count > 0;
        }

        private void SaveLayoutCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "All files|*.*", Title = "Select layout file name", FileName = "laytest.xml" };
            if (dlg.ShowDialog() == true)
            {
				FileServiceProviderWpf.SerializeDataToFile(dlg.FileName, gg_Area.ExtractSerializationData());
            }
        }
        #endregion

        #region LoadLayoutCommand

        private static readonly RoutedCommand LoadLayoutCommand = new RoutedCommand();
        private static void LoadLayoutCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void LoadLayoutCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "All files|*.*", Title = "Select layout file", FileName = "laytest.xml" };
            if (dlg.ShowDialog() != true) return;
            try
            {
				gg_Area.RebuildFromSerializationData(FileServiceProviderWpf.DeserializeDataFromFile(dlg.FileName));
                gg_Area.SetVerticesDrag(true, true);
                gg_Area.UpdateAllEdges();
                gg_zoomctrl.ZoomToFill();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to load layout file:\n {0}", ex));
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

            gg_but_relayout.Command = new SimpleCommand(GGRelayoutCommandCanExecute, GgRelayoutCommandExecute);
        }

        #endregion

        void gg_async_Checked(object sender, RoutedEventArgs e)
        {
            gg_Area.LogicCore.AsyncAlgorithmCompute = gg_async.IsChecked != null;
        }

        private void gg_saveAsPngImage_Click(object sender, RoutedEventArgs e)
        {
            gg_Area.ExportAsImage(ImageType.PNG);
        }

        private void gg_printlay_Click(object sender, RoutedEventArgs e)
        {
            gg_Area.PrintDialog("GraphX layout printing");
        }

        private void gg_vertexCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = CustomHelper.IsIntegerInput(e.Text) && Convert.ToInt32(e.Text) <= ShowcaseHelper.DATASOURCE_SIZE;
        }

        private void gg_layalgo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var late = (LayoutAlgorithmTypeEnum)gg_layalgo.SelectedItem;
            gg_Area.LogicCore.DefaultLayoutAlgorithm = late;
            if (late == LayoutAlgorithmTypeEnum.BoundedFR)
                gg_Area.LogicCore.DefaultLayoutAlgorithmParams
                    = gg_Area.LogicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.BoundedFR);
            if (late == LayoutAlgorithmTypeEnum.FR)
                gg_Area.LogicCore.DefaultLayoutAlgorithmParams
                    = gg_Area.LogicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.FR);
        }

        private void gg_useExternalLayAlgo_Checked(object sender, RoutedEventArgs e)
        {
            if (gg_useExternalLayAlgo.IsChecked == true)
            {
                var graph = gg_Area.LogicCore.Graph ?? ShowcaseHelper.GenerateDataGraph(Convert.ToInt32(gg_vertexCount.Text));
                gg_Area.LogicCore.Graph = graph;
                AssignExternalLayoutAlgorithm(graph);
            }
            else gg_Area.LogicCore.ExternalLayoutAlgorithm = null;
        }

        private void AssignExternalLayoutAlgorithm(BidirectionalGraph<DataVertex, DataEdge> graph)
        {
            gg_Area.LogicCore.ExternalLayoutAlgorithm = gg_Area.LogicCore.AlgorithmFactory.CreateLayoutAlgorithm(LayoutAlgorithmTypeEnum.ISOM, graph);
        }

        private void gg_oralgo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var core = gg_Area.LogicCore;
            core.DefaultOverlapRemovalAlgorithm = (OverlapRemovalAlgorithmTypeEnum)gg_oralgo.SelectedItem;
            if (core.DefaultOverlapRemovalAlgorithm == OverlapRemovalAlgorithmTypeEnum.FSA || core.DefaultOverlapRemovalAlgorithm == OverlapRemovalAlgorithmTypeEnum.OneWayFSA)
            {
                core.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 30;
                core.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 30;
            }
        }

        private void gg_useExternalORAlgo_Checked(object sender, RoutedEventArgs e)
        {
            gg_Area.LogicCore.ExternalOverlapRemovalAlgorithm = gg_useExternalORAlgo.IsChecked == true ? gg_Area.LogicCore.AlgorithmFactory.CreateOverlapRemovalAlgorithm(OverlapRemovalAlgorithmTypeEnum.FSA, null) : null;
        }

        private void gg_eralgo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gg_Area.LogicCore.DefaultEdgeRoutingAlgorithm = (EdgeRoutingAlgorithmTypeEnum)gg_eralgo.SelectedItem;
        }

        private void gg_useExternalERAlgo_Checked(object sender, RoutedEventArgs e)
        {
            if (gg_useExternalERAlgo.IsChecked == true)
            {
                var graph = gg_Area.LogicCore.Graph ?? ShowcaseHelper.GenerateDataGraph(Convert.ToInt32(gg_vertexCount.Text));
                gg_Area.LogicCore.Graph = graph;
                gg_Area.GetLogicCore<LogicCoreExample>().ExternalEdgeRoutingAlgorithm = gg_Area.LogicCore.AlgorithmFactory.CreateEdgeRoutingAlgorithm(EdgeRoutingAlgorithmTypeEnum.SimpleER, new Rect(gg_Area.DesiredSize.ToGraphX()), graph, null, null);
            }
            else gg_Area.GetLogicCore<LogicCoreExample>().ExternalEdgeRoutingAlgorithm = null;
        }

        void gg_Area_RelayoutFinished(object sender, EventArgs e)
        {
            if (gg_Area.LogicCore.AsyncAlgorithmCompute)
                gg_loader.Visibility = Visibility.Collapsed;
            gg_zoomctrl.ZoomToFill();
            gg_zoomctrl.Mode = ZoomControlModes.Custom;
        }

        /// <summary>
        /// Use this event in case we have chosen async computation
        /// </summary>
        void gg_Area_GenerateGraphFinished(object sender, EventArgs e)
        {
            if(!gg_Area.EdgesList.Any())
                gg_Area.GenerateAllEdges();
            if (gg_Area.LogicCore.AsyncAlgorithmCompute)
                gg_loader.Visibility = Visibility.Collapsed;

            gg_zoomctrl.ZoomToFill();
            gg_zoomctrl.Mode = ZoomControlModes.Custom;
        }

        private void gg_but_randomgraph_Click(object sender, RoutedEventArgs e)
        {
            gg_Area.ClearLayout();
            var graph = ShowcaseHelper.GenerateDataGraph(Convert.ToInt32(gg_vertexCount.Text));
            graph.AddEdge(new DataEdge(graph.Vertices.First(), graph.Vertices.First()));
            //assign graph again as we need to update Graph param inside and i have no independent examples
            if (gg_Area.LogicCore.ExternalLayoutAlgorithm != null)
                AssignExternalLayoutAlgorithm(graph);

            if (gg_Area.LogicCore.AsyncAlgorithmCompute)
                gg_loader.Visibility = Visibility.Visible;

            //supplied graph will be automaticaly be assigned to GraphArea::LogicCore.Graph property
            gg_Area.GenerateGraph(graph);
        }
    }
}
