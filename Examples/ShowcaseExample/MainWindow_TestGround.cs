using GraphX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GraphX.GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using GraphX.Logic;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Tree;
using GraphX.GraphSharp.Algorithms.Layout;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ShowcaseExample
{
    public partial class MainWindow
    {
        private LogicCoreExample tst;
        public LogicCoreExample TSTLC { get { return tst; } set { tst = value; OnPropertyChanged("TSTLC"); } }

        private void TestGround_Constructor()
        {
            tst_but_gen.Click += tst_but_gen_Click;
            tst_but_action.Click += tst_but_action_Click;
            //tst_Area.UseNativeObjectArrange = false;
            tst_Area.EnableVisualPropsRecovery = true;
            tst_Area.SetVerticesMathShape(VertexShape.Rectangle);
            tst_Area.SetVerticesDrag(false);
            tst_Area.ShowAllVerticesLabels();
            tst_Area.VertexSelected += tst_Area_VertexSelected;
            tst_Area.VertexMouseUp += tst_Area_VertexMouseUp;
        }

        void tst_Area_VertexMouseUp(object sender, GraphX.Models.VertexSelectedEventArgs args)
        {
            if (args.VertexControl != _firstVertex && _secondVertex == null)
            {
                _secondVertex = args.VertexControl;
                var logic = tst_Area.GetLogicCore<LogicCoreExample>();
                var dataEdge = new DataEdge(_firstVertex.Vertex as DataVertex, _secondVertex.Vertex as DataVertex);
                logic.Graph.AddEdge(dataEdge);
                tst_Area.AddEdge(dataEdge, new EdgeControl(_firstVertex, _secondVertex, dataEdge));
            }
        }

        private VertexControl _firstVertex;
        private VertexControl _secondVertex;

        private bool _action;
        void tst_Area_VertexSelected(object sender, GraphX.Models.VertexSelectedEventArgs args)
        {
            _firstVertex = args.VertexControl;
        }

        void tst_but_action_Click(object sender, RoutedEventArgs e)
        {
            //tst_Area.GetLogicCore<LogicCoreExample>().Graph.Edges.First().Text = "LoadLayoutCommand";
            //tst_Area.DeserializeFromFile("1.txt");
            _action = !_action;
        }

        private GraphExample GenerateTestGraph()
        {
            var graph = new GraphExample();
            var v1 = new DataVertex() { Text = "Test1", ID = 1};
            graph.AddVertex(v1);
            var v2 = new DataVertex() { Text = "Test2", ID = 2 };
            graph.AddVertex(v2);
            var v3 = new DataVertex() { Text = "Test3", ID = 3, SkipProcessing = _action };
            graph.AddVertex(v3);


            graph.AddEdge(new DataEdge(v1, v2, 100) { Text = Rand.Next().ToString() });


            
            return graph;

        }

        void tst_but_gen_Click(object sender, RoutedEventArgs e)
        {
            var graph = GenerateTestGraph();
            var logic = new LogicCoreExample {Graph = graph};
            logic.EnableParallelEdges = false;
            logic.ParallelEdgeDistance = 15;
            tst_Area.LogicCore = logic;

            tst_Area.ShowAllEdgesArrows(true);
            tst_Area.ShowAllEdgesLabels();
            tst_Area.SetVerticesDrag(true, true);
            var layParams = new LinLogLayoutParameters { IterationCount = 100 };
            logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.SimpleRandom;
            logic.DefaultLayoutAlgorithmParams = layParams;
            var overlapParams = new OverlapRemovalParameters { HorizontalGap = 100, VerticalGap = 100 };
            logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logic.DefaultOverlapRemovalAlgorithmParams = overlapParams;
            IExternalEdgeRouting<DataVertex, DataEdge> erParams = null;
            //logic.ExternalEdgeRoutingAlgorithm = 
            TSTLC = logic;

            tst_Area.GenerateGraph(graph, true);
            //tst_Area.VertexList[v1].Visibility = System.Windows.Visibility.Collapsed;
            //tst_Area.VertexList[v2].Visibility = System.Windows.Visibility.Collapsed;
            //tst_Area.VertexList[v3].Visibility = System.Windows.Visibility.Collapsed;
            //tst_Area.VertexList[v4].SetPosition(new Point(0, 0));
            //tst_Area.ShowAllEdgesLabels();
            //tst_Area.AlignAllEdgesLabels();
            tst_zoomctrl.ZoomToFill();

           /* var img = new BitmapImage(new Uri(@"pack://application:,,,/ShowcaseExample;component/Images/birdy.png", UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad };
            GraphAreaBase.SetX(img, -100);
            GraphAreaBase.SetY(img, -100);
            var image = new Image() { Source = img, Width = 100, Height = 100 };
            var border = new Border() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(2), Background = Brushes.Black, Width = 100, Height = 100 };
            image.Visibility = System.Windows.Visibility.Visible;
            border.Visibility = System.Windows.Visibility.Visible;
            tst_Area.InsertCustomChildControl(0, image);
            tst_Area.InsertCustomChildControl(0, border);*/
        }
      

    }
}
