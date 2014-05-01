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
            //tst_Area.UseNativeObjectArrange = false;

        }

        private GraphExample GenerateTestGraph()
        {
            var graph = new GraphExample();
            var v1 = new DataVertex() { Text = "Test1", ID = 1 };
            graph.AddVertex(v1);
            var v2 = new DataVertex() { Text = "Test2", ID = 2 };
            graph.AddVertex(v2);
            var v3 = new DataVertex() { Text = "Test3", ID = 3 };
            graph.AddVertex(v3);
            var v4 = new DataVertex() { Text = "Test4", ID = 4 };
            graph.AddVertex(v4);

            graph.AddEdge(new DataEdge(v1, v2, 100) { ToolTipText = "FFFFFF" });
            graph.AddEdge(new DataEdge(v1, v2, 100) { ToolTipText = "FFFFFF" });
            graph.AddEdge(new DataEdge(v1, v2, 100) { ToolTipText = "FFFFFF" }); 
            graph.AddEdge(new DataEdge(v1, v1, 100) { ToolTipText = "AAA" });

            return graph;

        }

        void tst_but_gen_Click(object sender, RoutedEventArgs e)
        {
            var graph = GenerateTestGraph();
            var logic = new LogicCoreExample {Graph = graph};

            logic.EnableParallelEdges = true;
            logic.ParallelEdgeDistance = 25;
            tst_Area.AlignAllEdgesLabels(true);
            tst_Area.ShowAllEdgesLabels(true);
            tst_Area.ShowAllEdgesArrows(true);
            tst_Area.EnableVisualPropsRecovery = true;
            tst_Area.SetVerticesMathShape(VertexShape.Rectangle);
            tst_Area.SetVerticesDrag(true, true);


            var layParams = new LinLogLayoutParameters { IterationCount = 100 };
            logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
            logic.DefaultLayoutAlgorithmParams = layParams;
            var overlapParams = new OverlapRemovalParameters { HorizontalGap = 100, VerticalGap = 100 };
            logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logic.DefaultOverlapRemovalAlgorithmParams = overlapParams;
            IExternalEdgeRouting<DataVertex, DataEdge> erParams = null;
            //logic.ExternalEdgeRoutingAlgorithm = 

            TSTLC = logic;
            tst_Area.GenerateGraph(graph, true);
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
