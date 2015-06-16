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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace METRO.SimpleGraph
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPageDebug : Page
    {
        public MainPageDebug()
        {
            InitializeComponent();

            butRelayout.Click += butRelayout_Click;
            butGenerate.Click += butGenerate_Click;
            graph.GenerateGraphFinished += OnFinishedLayout;
            graph.RelayoutFinished += OnFinishedLayout;
            graph.AlignAllEdgesLabels();
            Loaded += MainPage_Loaded;
            cboxDebug.ItemsSource = Enum.GetValues(typeof(DebugItems)).Cast<DebugItems>();
            cboxDebug.SelectionChanged += cboxDebug_SelectionChanged;
        }

        #region Debug implementation
        void cboxDebug_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //graph.VertexList.Values.ForEach(a => a.VertexConnectionPointsList.ForEach(b => b.Hide()));
            graph.ClearLayout();
            graph.LogicCore.Graph.Clear();
            graph.LogicCore.EnableParallelEdges = false;
            graph.LogicCore.ParallelEdgeDistance = 25;
            graph.LogicCore.EdgeCurvingEnabled = false;

            graph.SetVerticesDrag(true, true);
            graph.SetVerticesMathShape(VertexShape.Circle);
            graph.ShowAllVerticesLabels(false);
            graph.ShowAllEdgesLabels(false);
            graph.AlignAllEdgesLabels(false);

            switch ((DebugItems)cboxDebug.SelectedItem)
            {
                case DebugItems.General:
                    DebugGeneral();
                    break;
                case DebugItems.EdgeLabels:
                    DebugEdgeLabels();
                    break;
                case DebugItems.VCP:
                    DebugVCP();
                    break;
            }

        }

        private void DebugVCP()
        {
            graph.LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;

            var dataVertex = new DataVertex("MyVertex 1") { VisualDiameter = 20 };
            graph.LogicCore.Graph.AddVertex(dataVertex);
            dataVertex = new DataVertex("MyVertex 2") { VisualDiameter = 20 };
            graph.LogicCore.Graph.AddVertex(dataVertex);

            var vlist = graph.LogicCore.Graph.Vertices.ToList();
            AddEdge((GraphExample)graph.LogicCore.Graph, 0, 1, vlist);
            graph.LogicCore.Graph.Edges.Last().SourceConnectionPointId = 1;
            graph.LogicCore.Graph.Edges.Last().TargetConnectionPointId = 1;

            graph.GenerateGraphAsync();
        }

        private void DebugGeneral()
        {

        }

        private void DebugEdgeLabels()
        {
            graph.LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            graph.ShowAllEdgesLabels();
            graph.AlignAllEdgesLabels();
            graph.UpdateEdgeLabelPosition(true);

            DebugCreateVertex("MyVertex 1");
            DebugCreateVertex("MyVertex 2");

            var vlist = graph.LogicCore.Graph.Vertices.ToList();
            AddEdge((GraphExample)graph.LogicCore.Graph, 0, 1, vlist);

            graph.GenerateGraphAsync();
        }

        private void DebugOnFinished()
        {
            switch ((DebugItems)cboxDebug.SelectedItem)
            {
                case DebugItems.General:
                    break;
                case DebugItems.EdgeLabels:
                    break;
                case DebugItems.VCP:
                    break;
            }
        }

        private DataVertex DebugCreateVertex(string name)
        {
            var x = new DataVertex(name) { VisualDiameter = 20 };
            graph.LogicCore.Graph.AddVertex(x);
            return x;
        }

        #endregion

        void OnFinishedLayout(object sender, EventArgs e)
        {
            zc.ZoomToFill();

            DebugOnFinished();
        }

        private async void butGenerate_Click(object sender, RoutedEventArgs e)
        {
            GraphAreaExample_Setup();

            try
            {
                await graph.GenerateGraphAsync(true);
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
                var t0 = DateTime.Now;
                await graph.RelayoutGraphAsync();
                Debug.WriteLine("Time elapsed: {0}", DateTime.Now - t0);
            }
            catch (OperationCanceledException)
            {
                // User may have canceled
            }
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitialSetup();
            GraphAreaExample_Setup();

            try
            {
                //await graph.GenerateGraphAsync(true);
                graph.PreloadVertexes(graph.LogicCore.Graph);
                var count = 0;
                foreach (var item in graph.VertexList.Values.ToList())
                {
                    if (count == 0)
                        item.SetPosition(0, 0);
                    if (count == 1)
                        item.SetPosition(400, 0);

                    count++;
                }
                graph.GenerateAllEdges();
                graph.SetVerticesDrag(true);
            }
            catch (OperationCanceledException)
            {
                // User may have canceled
            }
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

            //debug


            dataGraph.AddVertex(new DataVertex("MyVertex " + 1) { ID = 1, VisualDiameter = 10, VisualInnerDiameter = 10 });
            dataGraph.AddVertex(new DataVertex("MyVertex " + 2) { ID = 2, VisualDiameter = 10, VisualInnerDiameter = 10 });
            var vlist = dataGraph.Vertices.ToList();
            AddEdge(dataGraph, 0, 1, vlist);
            return dataGraph;
        }

        private void InitialSetup()
        {
            var logicCore = new GXLogicCoreExample();
            graph.LogicCore = logicCore;

            var layParams = new LinLogLayoutParameters { IterationCount = 100 };
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.SimpleRandom;
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
            var dataGraph = GraphExample_Setup();
            logicCore.Graph = dataGraph;

            logicCore.EnableParallelEdges = true;
            logicCore.ParallelEdgeDistance = 25;
            logicCore.EdgeCurvingEnabled = true;

            graph.SetVerticesDrag(true, true);
            graph.SetVerticesMathShape(VertexShape.Circle);
            graph.ShowAllVerticesLabels();
            graph.ShowAllEdgesLabels();

            //DEBUG
            graph.UseLayoutRounding = false;
            zc.UseLayoutRounding = false;

            graph.ShowAllEdgesLabels(false);
            graph.LogicCore.ExternalEdgeRoutingAlgorithm = null;
            graph.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            graph.SetVerticesMathShape(VertexShape.Rectangle);
            //graph.MouseOverAnimation = AnimationFactory.CreateMouseOverAnimation(MouseOverAnimation.Scale);

            /*cboxLayout_SelectionChanged(null, null);
            cboxOverlap_SelectionChanged(null, null);
            cboxEdgeRouting_SelectionChanged(null, null);*/
        }

        void MoveAnimation_Completed(object sender, EventArgs e)
        {
            zc.ZoomToFill();
        }
    }
}
