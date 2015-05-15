using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GraphX;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using GraphX.WPF.Controls.Animations;
using GraphX.WPF.Controls.Models;
using ShowcaseApp.WPF.Models;
using Point = GraphX.Measure.Point;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for DebugGraph.xaml
    /// </summary>
    public partial class DebugGraph : UserControl
    {
        public DebugGraph()
        {
            InitializeComponent();
            DataContext = this;
            butRun.Click += butRun_Click;
            butTest.Click += butTest_Click;
            butRelay.Click +=ButRelayOnClick;
        }

        private void ButRelayOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            dg_Area.RelayoutGraph(true);
        }

        void butTest_Click(object sender, RoutedEventArgs e)
        {
            var win = new Window {Content = new EditorGraph()};
            win.ShowDialog();
        }

        void butRun_Click(object sender, RoutedEventArgs e)
        {
           /* var lc = new LogicCoreExample() {Graph = ShowcaseHelper.GenerateDataGraph(25)};

            //lc.Graph.AddVertex(new DataVertex("Test vertex"));
            dg_Area.LogicCore = lc;
            dg_Area.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.Bundling;
            dg_Area.LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            dg_Area.ShowAllEdgesLabels(true);
            dg_Area.AlignAllEdgesLabels(true);
            dg_Area.GenerateGraph(true);*/

            var logicCore = new LogicCoreExample() { Graph = ShowcaseHelper.GenerateDataGraph(25) };
            foreach (var item in logicCore.Graph.Vertices.Take(4))
            {
                item.SkipProcessing = ProcessingOptionEnum.Freeze;
            }

            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Circular;
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams = logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            logicCore.AsyncAlgorithmCompute = false;
            logicCore.EdgeCurvingEnabled = true;

            dg_Area.LogicCore = logicCore;
            dg_Area.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(0.5));
            dg_Area.ShowAllEdgesLabels(true);
            dg_Area.AlignAllEdgesLabels(true);
            dg_Area.ShowAllEdgesArrows(true);
            dg_Area.SetVerticesDrag(true, true);
            //dg_Area.InvalidateVisual();

            dg_Area.GenerateGraph(true);
            foreach (var item in logicCore.Graph.Vertices.Take(4))
            {
                dg_Area.VertexList[item].SetPosition(new System.Windows.Point());
            }

            //dg_Area.RelayoutGraph();
            //dg_zoomctrl.ZoomToFill();

            //dg_Area.UpdateAllEdges();
            //dg_Area.UpdateLayout();
            //dg_Area.InvalidateVisual();
            //dg_Area.RelayoutGraph();
            dg_zoomctrl.ZoomToFill();
        }
    }
}
