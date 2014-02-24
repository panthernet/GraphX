using GraphX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GraphX.GraphSharp.Algorithms.Layout.Simple.FDP;
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
            tst_Area.EnableVisualPropsRecovery = true;
            tst_Area.SetVerticesMathShape(VertexShape.Rectangle);
            tst_Area.SetVerticesDrag(true, true);
        }

        void tst_but_gen_Click(object sender, RoutedEventArgs e)
        {
            var _graph = new GraphExample();
            var v1 = new DataVertex() { Text = "Test1", ID = 1 };
            _graph.AddVertex(v1);
            var v2 = new DataVertex() { Text = "Test2", ID = 2 };
            _graph.AddVertex(v2);
            var v3 = new DataVertex() { Text = "Test3", ID = 3 };
            _graph.AddVertex(v3);
            var v4 = new DataVertex() { Text = "Test4", ID = 4 };
            _graph.AddVertex(v4);

            _graph.AddEdge(new DataEdge(v1, v2, 1) { ID = 1000 });
            _graph.AddEdge(new DataEdge(v1, v2, 1) { ID = 1 });


            _graph.AddEdge(new DataEdge(v1, v4, 1) { ID = 1000 });
            _graph.AddEdge(new DataEdge(v1, v4, 1) { ID = 1 });
            _graph.AddEdge(new DataEdge(v1, v4, 1) { ID = 2 });
            _graph.AddEdge(new DataEdge(v2, v4, 1) { ID = 1001 });
            _graph.AddEdge(new DataEdge(v3, v4, 1) { ID = 1002 });
            _graph.AddEdge(new DataEdge(v3, v4, 1) { ID = 1003 });
            _graph.AddEdge(new DataEdge(v4, v3, 1) { ID = 1004 });
            _graph.AddEdge(new DataEdge(v4, v3, 1) { ID = 1005 });
            _graph.AddEdge(new DataEdge(v4, v3, 1) { ID = 1006 });

            tst_Area.ShowAllEdgesArrows(true);

            var ergTreeLayoutParameters = new KKLayoutParameters { };

            var logic = new LogicCoreExample();
            TSTLC = logic;
            logic.Graph = _graph;
            logic.EnableParallelEdges = true;

            logic.ParallelEdgeDistance = 15;

            logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            logic.DefaultLayoutAlgorithmParams = ergTreeLayoutParameters;

            logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logic.DefaultOverlapRemovalAlgorithmParams = logic.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);

            ((GraphX.GraphSharp.Algorithms.OverlapRemoval.OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 140;
            ((GraphX.GraphSharp.Algorithms.OverlapRemoval.OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 140;
            tst_Area.GenerateGraph(_graph, true);
            //tst_Area.VertexList[v1].Visibility = System.Windows.Visibility.Collapsed;
            //tst_Area.VertexList[v2].Visibility = System.Windows.Visibility.Collapsed;
            //tst_Area.VertexList[v3].Visibility = System.Windows.Visibility.Collapsed;
            //tst_Area.VertexList[v4].SetPosition(new Point(0, 0));
            tst_Area.ShowAllEdgesLabels();
            tst_Area.AlignAllEdgesLabels();
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
