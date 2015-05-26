using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphX;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.EdgeRouting;
using GraphX.Controls;
using ShowcaseApp.WPF.Models;
using Rect = GraphX.Measure.Rect;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for EdgeRoutingGraph.xaml
    /// </summary>
    public partial class EdgeRoutingGraph : UserControl, INotifyPropertyChanged
    {
        private PathFinderEdgeRoutingParameters _pfPrms;
        public PathFinderEdgeRoutingParameters PfErParameters { get { return _pfPrms; } set { _pfPrms = value; OnPropertyChanged("PfErParameters"); } }
        private SimpleERParameters _simplePrms;
        public SimpleERParameters SimpleErParameters { get { return _simplePrms; } set { _simplePrms = value; OnPropertyChanged("SimpleErParameters"); } }
        private BundleEdgeRoutingParameters _bundlePrms;
        public BundleEdgeRoutingParameters BundleEdgeRoutingParameters { get { return _bundlePrms; } set { _bundlePrms = value; OnPropertyChanged("BundleEdgeRoutingParameters"); } }

        private readonly LogicCoreExample _logicCore;

        public EdgeRoutingGraph()
        {
            InitializeComponent();
            DataContext = this;
            _logicCore = new LogicCoreExample();
            erg_Area.LogicCore = _logicCore;
            erg_Area.LogicCore.ParallelEdgeDistance = 20;

            erg_showEdgeArrows.IsChecked = true;
            BundleEdgeRoutingParameters = (BundleEdgeRoutingParameters)_logicCore.AlgorithmFactory.CreateEdgeRoutingParameters(EdgeRoutingAlgorithmTypeEnum.Bundling);
            SimpleErParameters = (SimpleERParameters)_logicCore.AlgorithmFactory.CreateEdgeRoutingParameters(EdgeRoutingAlgorithmTypeEnum.SimpleER);
            PfErParameters = (PathFinderEdgeRoutingParameters)_logicCore.AlgorithmFactory.CreateEdgeRoutingParameters(EdgeRoutingAlgorithmTypeEnum.PathFinder);

            erg_pfprm_formula.ItemsSource = Enum.GetValues(typeof(PathFindAlgorithm)).Cast<PathFindAlgorithm>();
            erg_pfprm_formula.SelectedIndex = 0;

            erg_but_randomgraph.Click += erg_but_randomgraph_Click;
            erg_but_relayout.Click += erg_but_relayout_Click;
            erg_useExternalERAlgo.Checked += erg_useExternalERAlgo_Checked;
            erg_useExternalERAlgo.Unchecked += erg_useExternalERAlgo_Checked;
            erg_dashstyle.ItemsSource = Enum.GetValues(typeof(EdgeDashStyle)).Cast<EdgeDashStyle>();
            erg_dashstyle.SelectedIndex = 0;
            erg_dashstyle.SelectionChanged += erg_dashstyle_SelectionChanged;
            erg_eralgo.ItemsSource = Enum.GetValues(typeof(EdgeRoutingAlgorithmTypeEnum)).Cast<EdgeRoutingAlgorithmTypeEnum>();
            erg_eralgo.SelectedIndex = 0;
            erg_eralgo.SelectionChanged += erg_eralgo_SelectionChanged;
            erg_prmsbox.Visibility = Visibility.Collapsed;
            erg_recalculate.Checked += erg_recalculate_Checked;
            erg_recalculate.Unchecked += erg_recalculate_Checked;
            erg_randomizeAll.Click += erg_randomizeAll_Click;
            erg_showEdgeArrows.Checked += erg_showEdgeArrows_Checked;
            erg_showEdgeArrows.Unchecked += erg_showEdgeArrows_Checked;
            erg_showEdgeLabels.Checked += erg_showEdgeLabels_Checked;
            erg_showEdgeLabels.Unchecked += erg_showEdgeLabels_Checked;
            erg_alignEdgeLabels.Checked += erg_alignEdgeLabels_Checked;
            erg_alignEdgeLabels.Unchecked += erg_alignEdgeLabels_Checked;
            erg_enableParallelEdges.Checked += erg_enableParallelEdges_Checked;
            erg_enableParallelEdges.Unchecked += erg_enableParallelEdges_Checked;


            erg_randomizeArrows.Click += erg_randomizeArrows_Click;
            erg_useCurves.Checked += erg_useCurves_Checked;
            erg_useCurves.Unchecked += erg_useCurves_Checked;
            ZoomControl.SetViewFinderVisibility(erg_zoomctrl, Visibility.Visible);
        }

        private void erg_toggleVertex_Click(object sender, RoutedEventArgs e)
        {
            if (erg_Area.VertexList.First().Value.Visibility == Visibility.Visible)
                foreach (var item in erg_Area.VertexList)
                    item.Value.Visibility = Visibility.Collapsed;
            else foreach (var item in erg_Area.VertexList)
                    item.Value.Visibility = Visibility.Visible;
        }

        void erg_enableParallelEdges_Checked(object sender, RoutedEventArgs e)
        {
            erg_Area.LogicCore.EnableParallelEdges = erg_enableParallelEdges.IsChecked != null && erg_enableParallelEdges.IsChecked.Value;
        }

        void erg_useCurves_Checked(object sender, RoutedEventArgs e)
        {
            //update edge curving property
            erg_Area.LogicCore.EdgeCurvingEnabled = erg_useCurves.IsChecked != null && erg_useCurves.IsChecked.Value;
            erg_Area.UpdateAllEdges();
        }

        void erg_randomizeArrows_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in erg_Area.EdgesList.ToList())
                item.Value.SetCurrentValue(EdgeControl.ShowArrowsProperty, Convert.ToBoolean(ShowcaseHelper.Rand.Next(0, 2)));
        }

        void erg_showEdgeLabels_Checked(object sender, RoutedEventArgs e)
        {
            erg_Area.ShowAllEdgesLabels(erg_showEdgeLabels.IsChecked != null && erg_showEdgeLabels.IsChecked.Value);
            erg_Area.InvalidateVisual();
        }

        void erg_alignEdgeLabels_Checked(object sender, RoutedEventArgs e)
        {
            erg_Area.AlignAllEdgesLabels(erg_alignEdgeLabels.IsChecked != null && erg_alignEdgeLabels.IsChecked.Value);
            erg_Area.InvalidateVisual();
        }

        void erg_showEdgeArrows_Checked(object sender, RoutedEventArgs e)
        {
            erg_Area.ShowAllEdgesArrows(erg_showEdgeArrows.IsChecked != null && erg_showEdgeArrows.IsChecked.Value);
        }


        void erg_randomizeAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in erg_Area.EdgesList.ToList())
            {
                var sarr = Enum.GetValues(typeof(EdgeDashStyle));
                item.Value.DashStyle = (EdgeDashStyle)sarr.GetValue(ShowcaseHelper.Rand.Next(0, sarr.Length - 1));
            }
        }

        void erg_recalculate_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in erg_Area.GetAllVertexControls())
                DragBehaviour.SetUpdateEdgesOnMove(item, erg_recalculate.IsChecked != null && erg_recalculate.IsChecked.Value);

        }

        void erg_dashstyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            erg_Area.SetEdgesDashStyle((EdgeDashStyle)erg_dashstyle.SelectedItem);
        }

        private void erg_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = CustomHelper.IsIntegerInput(e.Text);
        }

        private void erg_to1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = CustomHelper.IsDoubleInput(e.Text);
            if (e.Handled) return;
            var res = 0.0;
            var textBox = sender as TextBox;
            if (textBox != null && !double.TryParse(textBox.Text, out res)) return;
            if (res < 0.0 || res > 1.0) e.Handled = false;
        }

        private void erg_tominus1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = CustomHelper.IsDoubleInput(e.Text);
            if (e.Handled) return;
            var res = 0.0;
            var textBox = sender as TextBox;
            if (textBox != null && !double.TryParse(textBox.Text, out res)) return;
            if (res < -1.0 || res > 0.0) e.Handled = false;
        }


        void erg_eralgo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            erg_recalculate.IsEnabled = true;
            if ((EdgeRoutingAlgorithmTypeEnum)erg_eralgo.SelectedItem == EdgeRoutingAlgorithmTypeEnum.None)
                erg_prmsbox.Visibility = Visibility.Collapsed;
            else
            {
                //clean prms
                erg_prmsbox.Visibility = Visibility.Visible;
                if ((EdgeRoutingAlgorithmTypeEnum)erg_eralgo.SelectedItem == EdgeRoutingAlgorithmTypeEnum.SimpleER)
                {
                    simpleer_prms_dp.Visibility = Visibility.Visible;
                    bundleer_prms_dp.Visibility = Visibility.Collapsed;
                    pfer_prms_dp.Visibility = Visibility.Collapsed;
                }
                if ((EdgeRoutingAlgorithmTypeEnum)erg_eralgo.SelectedItem == EdgeRoutingAlgorithmTypeEnum.PathFinder)
                {
                    simpleer_prms_dp.Visibility = Visibility.Collapsed;
                    bundleer_prms_dp.Visibility = Visibility.Collapsed;
                    pfer_prms_dp.Visibility = Visibility.Visible;
                }
                if ((EdgeRoutingAlgorithmTypeEnum)erg_eralgo.SelectedItem == EdgeRoutingAlgorithmTypeEnum.Bundling)
                {
                    simpleer_prms_dp.Visibility = Visibility.Collapsed;
                    bundleer_prms_dp.Visibility = Visibility.Visible;
                    pfer_prms_dp.Visibility = Visibility.Collapsed;
                    //bundling doesn't support single edge routing
                    erg_recalculate.IsChecked = false;
                    erg_recalculate.IsEnabled = false;
                }
            }
            //(Accordion.Items[1] as AccordionItem).IsSelected = true;
            Accordion.UpdateLayout();
            (Accordion.Items[0] as AccordionItem).UpdateLayout();
        }

        void erg_useExternalERAlgo_Checked(object sender, RoutedEventArgs e)
        {
            if (erg_useExternalERAlgo.IsChecked == true)
            {
                if (erg_Area.LogicCore.Graph == null) erg_but_randomgraph_Click(null, null);
                erg_Area.GetLogicCore<LogicCoreExample>().ExternalEdgeRoutingAlgorithm = erg_Area.LogicCore.AlgorithmFactory.CreateEdgeRoutingAlgorithm(EdgeRoutingAlgorithmTypeEnum.SimpleER, new Rect(0, 0, erg_Area.DesiredSize.Width, erg_Area.DesiredSize.Height), erg_Area.LogicCore.Graph, null, null);
            }
            else erg_Area.GetLogicCore<LogicCoreExample>().ExternalEdgeRoutingAlgorithm = null;
        }

        void erg_but_relayout_Click(object sender, RoutedEventArgs e)
        {
            switch ((EdgeRoutingAlgorithmTypeEnum)erg_eralgo.SelectedItem)
            {
                case EdgeRoutingAlgorithmTypeEnum.PathFinder:
                    erg_Area.RelayoutGraph();
                    break;
                case EdgeRoutingAlgorithmTypeEnum.Bundling:
                    erg_Area.RelayoutGraph();
                    break;
                default:
                    erg_Area.GenerateGraph(erg_Area.LogicCore.Graph, true);
                    break;
            }
        }

        private Dictionary<DataVertex, Point> GenerateRandomVertices(GraphExample graph, int index, int count, int minX, int maxX, int minY, int maxY)
        {
            var list = graph.Vertices.ToList();
            var vertexPositions = new Dictionary<DataVertex, Point>();
            for (var i = index; i < index + count; i++)
            {
                var vertex = list[i];
                var vc = new VertexControl(vertex);
                erg_Area.AddVertex(vertex, vc);
                vertexPositions[vertex] = new Point(ShowcaseHelper.Rand.Next(minX, maxX), ShowcaseHelper.Rand.Next(minY, maxY));
                vc.SetPosition(vertexPositions[vertex]);
            }
            return vertexPositions;
        }

        void erg_but_randomgraph_Click(object sender, RoutedEventArgs e)
        {

            if ((EdgeRoutingAlgorithmTypeEnum)erg_eralgo.SelectedItem != EdgeRoutingAlgorithmTypeEnum.Bundling)
            {
                var gr = ShowcaseHelper.GenerateDataGraph(30);

                if (erg_Area.LogicCore.EnableParallelEdges)
                {
                    if (erg_Area.VertexList.Count() < 2)
                    {
                        var v1 = new DataVertex(); gr.AddVertex(v1);
                        var v2 = new DataVertex(); gr.AddVertex(v2);
                        gr.AddEdge(new DataEdge(v1, v2) { Text = string.Format("{0} -> {1}", v1.Text, v2.Text) });
                        gr.AddEdge(new DataEdge(v2, v1) { Text = string.Format("{0} -> {1}", v2.Text, v1.Text) });
                    }
                    else
                    {
                        var v1 = gr.Vertices.ToList()[0];
                        var v2 = gr.Vertices.ToList()[1];
                        gr.AddEdge(new DataEdge(v1, v2) { Text = string.Format("{0} -> {1}", v1.Text, v2.Text) });
                        gr.AddEdge(new DataEdge(v2, v1) { Text = string.Format("{0} -> {1}", v2.Text, v1.Text) });
                    }
                }

                erg_Area.GetLogicCore<LogicCoreExample>().DefaultEdgeRoutingAlgorithm = (EdgeRoutingAlgorithmTypeEnum)erg_eralgo.SelectedItem;
                switch ((EdgeRoutingAlgorithmTypeEnum)erg_eralgo.SelectedItem)
                {
                    case EdgeRoutingAlgorithmTypeEnum.SimpleER:
                        erg_Area.GetLogicCore<LogicCoreExample>().DefaultEdgeRoutingAlgorithmParams = SimpleErParameters;
                        break;
                    case EdgeRoutingAlgorithmTypeEnum.PathFinder:
                        erg_Area.GetLogicCore<LogicCoreExample>().DefaultEdgeRoutingAlgorithmParams = PfErParameters;
                        break;
                }

                erg_Area.GetLogicCore<LogicCoreExample>().DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.SimpleRandom;
                erg_Area.GetLogicCore<LogicCoreExample>().DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
                erg_Area.GetLogicCore<LogicCoreExample>().DefaultOverlapRemovalAlgorithmParams = erg_Area.LogicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);

                erg_Area.GenerateGraph(gr, true);
                erg_Area.SetVerticesDrag(true, true);
                erg_zoomctrl.ZoomToFill();

                return;
            }

            erg_Area.RemoveAllEdges();
            erg_Area.RemoveAllVertices();
            //generate graph
            var graph = new GraphExample();
            foreach (var item in ShowcaseHelper.DataSource.Take(120))
                graph.AddVertex(new DataVertex(item.Text) { ID = item.ID });

            var vlist = graph.Vertices.ToList();
            foreach (var item in vlist)
            {
                if (ShowcaseHelper.Rand.Next(0, 50) > 25) continue;
                var vertex2 = vlist[ShowcaseHelper.Rand.Next(0, graph.VertexCount - 1)];
                graph.AddEdge(new DataEdge(item, vertex2, ShowcaseHelper.Rand.Next(1, 50)) { ToolTipText = string.Format("{0} -> {1}", item, vertex2) });
            }

            //generate vertices

            var vertexPositions = GenerateRandomVertices(graph, 0, 40, 0, 2000, 0, 2000).ToDictionary(item => item.Key, item => item.Value);
            foreach (var item in GenerateRandomVertices(graph, 40, 40, 5000, 7000, 3000, 4000))
                vertexPositions.Add(item.Key, item.Value);
            foreach (var item in GenerateRandomVertices(graph, 80, 40, 500, 2500, 6000, 9000))
                vertexPositions.Add(item.Key, item.Value);
            erg_Area.LogicCore.Graph = graph;
            UpdateLayout();

            erg_Area.SetVerticesDrag(true);
            _logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Custom;
            _logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.Bundling;
            _logicCore.DefaultEdgeRoutingAlgorithmParams = BundleEdgeRoutingParameters;
            erg_Area.GenerateGraph(true);

            erg_zoomctrl.ZoomToFill();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if(PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
