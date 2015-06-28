using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using METRO.SimpleGraph.Models;
using QuickGraph.Graphviz.Dot;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace METRO.SimpleGraph
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            cboxLayout.ItemsSource = Enum.GetValues(typeof(LayoutAlgorithmTypeEnum)).Cast<LayoutAlgorithmTypeEnum>();//.Where(a=> a != LayoutAlgorithmTypeEnum.FR && a != LayoutAlgorithmTypeEnum.BoundedFR).ToArray();
            cboxOverlap.ItemsSource = Enum.GetValues(typeof(OverlapRemovalAlgorithmTypeEnum)).Cast<OverlapRemovalAlgorithmTypeEnum>();
            cboxEdgeRouting.ItemsSource = Enum.GetValues(typeof(EdgeRoutingAlgorithmTypeEnum)).Cast<EdgeRoutingAlgorithmTypeEnum>();

            cboxLayout.SelectedItem = LayoutAlgorithmTypeEnum.LinLog;
            cboxOverlap.SelectedItem = OverlapRemovalAlgorithmTypeEnum.FSA;
            cboxEdgeRouting.SelectedItem = EdgeRoutingAlgorithmTypeEnum.None;

            cboxLayout.SelectionChanged += cboxLayout_SelectionChanged;
            cboxOverlap.SelectionChanged += cboxOverlap_SelectionChanged;
            cboxEdgeRouting.SelectionChanged += cboxEdgeRouting_SelectionChanged;

            butRelayout.Click += butRelayout_Click;
            butGenerate.Click += butGenerate_Click;
            graph.GenerateGraphFinished += OnFinishedLayout;
            graph.RelayoutFinished += OnFinishedLayout;
            graph.AlignAllEdgesLabels();
            graph.ControlsDrawOrder = ControlDrawOrder.VerticesOnTop;
            Loaded += MainPage_Loaded;
        }

        void OnFinishedLayout(object sender, EventArgs e)
        {
            zc.ZoomToFill();
        }

        private async void butGenerate_Click(object sender, RoutedEventArgs e)
        {
            GraphAreaExample_Setup();
            try
            {
                await graph.GenerateGraphAsync();
            }
            catch (OperationCanceledException)
            {
                // User may have canceled
            }
        }

        async void butRelayout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await graph.RelayoutGraphAsync();
            }
            catch (OperationCanceledException)
            {
                // User may have canceled
            }
        }

        void cboxEdgeRouting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graph.LogicCore == null) return;
            graph.LogicCore.DefaultEdgeRoutingAlgorithm = (EdgeRoutingAlgorithmTypeEnum)cboxEdgeRouting.SelectedItem;
        }

        void cboxOverlap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graph.LogicCore == null) return;
            graph.LogicCore.DefaultOverlapRemovalAlgorithm = (OverlapRemovalAlgorithmTypeEnum)cboxOverlap.SelectedItem;
        }

        void cboxLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(graph.LogicCore == null) return;
            var late = (LayoutAlgorithmTypeEnum) cboxLayout.SelectedItem;
            graph.LogicCore.DefaultLayoutAlgorithm = late;
            if (late == LayoutAlgorithmTypeEnum.BoundedFR)
                graph.LogicCore.DefaultLayoutAlgorithmParams
                    = graph.LogicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.BoundedFR);
            if (late == LayoutAlgorithmTypeEnum.FR)
                graph.LogicCore.DefaultLayoutAlgorithmParams
                    = graph.LogicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.FR);
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitialSetup();
            GraphAreaExample_Setup();

            try
            {
                await graph.GenerateGraphAsync();
            }
            catch (OperationCanceledException)
            {
                // User may have canceled
            }

            //graph.RelayoutGraph(true);
            //zc.ZoomToFill();
            //graph.VertexList.Values.ToList()[0].SetPosition(new Point(0, 0));
            //graph.VertexList.Values.ToList()[1].SetPosition(new Point(100, 0));
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void AddEdge(GraphExample igraph, int index1, int index2, IReadOnlyList<DataVertex> vlist)
        {
            var dataEdge = new DataEdge(vlist[index1], vlist[index2])
            {
                Text = string.Format("Edge {0}{1}", vlist[index1].ID, vlist[index2].ID),
                VisualEdgeThickness = _rnd.Next(1, 4),
                VisualEdgeTransparency = 1.0,
                VisualColor = "#ffffff"
            };
            igraph.AddEdge(dataEdge);
        }

        private readonly Random _rnd = new Random();
        private GraphExample GraphExample_Setup()
        {
            var dataGraph = new GraphExample();
            var vlist = new List<DataVertex>();;

            //debug
           /* dataGraph.AddVertex(new DataVertex("MyVertex " + 1) { ID = 1, VisualDiameter = 10, VisualInnerDiameter = 10 });
            dataGraph.AddVertex(new DataVertex("MyVertex " + 2) { ID = 2, VisualDiameter = 10, VisualInnerDiameter = 10 });
            vlist = dataGraph.Vertices.ToList();
            AddEdge(dataGraph, 0, 1, vlist);
            return dataGraph;*/



            switch ((LayoutAlgorithmTypeEnum)cboxLayout.SelectedItem)
            {
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                case LayoutAlgorithmTypeEnum.Sugiyama:
                case LayoutAlgorithmTypeEnum.BoundedFR:
                case LayoutAlgorithmTypeEnum.FR:
                case LayoutAlgorithmTypeEnum.Tree:
                    for (int i = 1; i < 14; i++)
                    {
                        var dataVertex = new DataVertex("MyVertex " + i) { ID = i, VisualDiameter = _rnd.Next(25, 50), VisualInnerDiameter = _rnd.Next(10, 22) };
                        dataGraph.AddVertex(dataVertex);
                    }
                    vlist = dataGraph.Vertices.ToList();
                    AddEdge(dataGraph, 0, 1, vlist);
                    AddEdge(dataGraph, 0, 0, vlist);

                    AddEdge(dataGraph, 0, 2, vlist);
                    AddEdge(dataGraph, 1, 3, vlist);
                    AddEdge(dataGraph, 1, 4, vlist);
                    AddEdge(dataGraph, 2, 5, vlist);
                    AddEdge(dataGraph, 2, 6, vlist);
                    AddEdge(dataGraph, 2, 7, vlist);

                    AddEdge(dataGraph, 8, 9, vlist);
                    AddEdge(dataGraph, 9, 10, vlist);
                    AddEdge(dataGraph, 10, 7, vlist);
                    AddEdge(dataGraph, 10, 11, vlist);
                    AddEdge(dataGraph, 10, 12, vlist);

                    break;
                default:
                     for (var i = 1; i < 11; i++)
                    {
                        var dataVertex = new DataVertex("MyVertex " + i) { ID = i, VisualDiameter = _rnd.Next(50, 100), VisualInnerDiameter = _rnd.Next(20, 45) };
                        if (i == 2)
                            dataVertex.LabelText += "\nMultiline!";
                        dataGraph.AddVertex(dataVertex);
                    }
                     vlist = dataGraph.Vertices.ToList();
                    AddEdge(dataGraph, 0, 1, vlist);

                    AddEdge(dataGraph, 1, 2, vlist);
                    AddEdge(dataGraph, 1, 3, vlist);
                    AddEdge(dataGraph, 1, 4, vlist);

                    AddEdge(dataGraph, 4, 5, vlist);
                    AddEdge(dataGraph, 4, 6, vlist);

                    AddEdge(dataGraph, 2, 7, vlist);
                    AddEdge(dataGraph, 2, 8, vlist);

                    AddEdge(dataGraph, 8, 9, vlist);

                    //add some cross references
                    AddEdge(dataGraph, 4, 2, vlist);
                    AddEdge(dataGraph, 4, 8, vlist);
                    AddEdge(dataGraph, 9, 2, vlist);

                    break;
            }

           /* foreach (var item in graph.EdgesList)
            {
                //item.Value.LabelVerticalOffset = -40;
                item.Value.LabelAngle = 45;
            }*/


            return dataGraph;

            /*ManipulationDelta += MainPage_ManipulationDelta;
            ManipulationMode = ManipulationModes.Scale;

            for (int i = 1; i < 10; i++)
            {
                var dataVertex = new DataVertex("MyVertex " + i) { ID = i };
                dataGraph.AddVertex(dataVertex);
            }

            var vlist = dataGraph.Vertices.ToList();
            //var dataEdge = new DataEdge(vlist[0], vlist[1]) { Text = string.Format("{0} -> {1}", vlist[0], vlist[1]) };
            //dataGraph.AddEdge(dataEdge);
            var dataEdge = new DataEdge(vlist[2], vlist[3]) { Text = "23" }; 
            dataGraph.AddEdge(dataEdge);
            dataEdge = new DataEdge(vlist[3], vlist[2]) { Text = "32" };
            dataGraph.AddEdge(dataEdge);

            return dataGraph;*/
        }

        void MainPage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

        }

        private void InitialSetup()
        {
            var logicCore = new GXLogicCoreExample();
            graph.LogicCore = logicCore;

            var layParams = new LinLogLayoutParameters { IterationCount = 100 };
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
            logicCore.DefaultLayoutAlgorithmParams = layParams;

            logicCore.DefaultOverlapRemovalAlgorithmParams = logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            graph.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromMilliseconds(500));
            graph.MoveAnimation.Completed += MoveAnimation_Completed;
        }

        private void GraphAreaExample_Setup()
        {

            var logicCore = graph.GetLogicCore<GXLogicCoreExample>();
            logicCore.Graph = GraphExample_Setup();

            switch ((LayoutAlgorithmTypeEnum) cboxLayout.SelectedItem)
            {
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                    logicCore.DefaultLayoutAlgorithmParams = new EfficientSugiyamaLayoutParameters { VertexDistance = 50 };
                    break;
            }


            switch ((LayoutAlgorithmTypeEnum)cboxLayout.SelectedItem)
            {
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                case LayoutAlgorithmTypeEnum.Sugiyama:
                case LayoutAlgorithmTypeEnum.BoundedFR:
                case LayoutAlgorithmTypeEnum.FR:
                case LayoutAlgorithmTypeEnum.Tree:
                    cboxEdgeRouting.SelectedItem = EdgeRoutingAlgorithmTypeEnum.SimpleER;
                    break;
                default:
                    cboxEdgeRouting.SelectedItem = EdgeRoutingAlgorithmTypeEnum.SimpleER;
                    break;
            }

            logicCore.EnableParallelEdges = true;
            logicCore.ParallelEdgeDistance = 25;
            logicCore.EdgeCurvingEnabled = true;

            graph.SetVerticesDrag(true, true);
            graph.SetVerticesMathShape(VertexShape.Circle);
            graph.ShowAllVerticesLabels();
            graph.ShowAllEdgesLabels();
        }

        void MoveAnimation_Completed(object sender, EventArgs e)
        {
            zc.ZoomToFill();
        }
    }
}
