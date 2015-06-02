using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for DebugGraph.xaml
    /// </summary>
    public partial class DebugGraph : UserControl, INotifyPropertyChanged
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
           // LoadLogEntries();
            //return;
           /* var lc = new LogicCoreExample() {Graph = ShowcaseHelper.GenerateDataGraph(25)};

            //lc.Graph.AddVertex(new DataVertex("Test vertex"));
            dg_Area.LogicCore = lc;
            dg_Area.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.Bundling;
            dg_Area.LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            dg_Area.ShowAllEdgesLabels(true);
            dg_Area.AlignAllEdgesLabels(true);
            dg_Area.GenerateGraph(true);*/

            var logicCore = new LogicCoreExample { Graph = ShowcaseHelper.GenerateDataGraph(5, false) };

            var vlist = logicCore.Graph.Vertices.ToList();
            var edge = new DataEdge(vlist[0], vlist[1]) { SourceConnectionPointId = 1, TargetConnectionPointId = 1 };
            logicCore.Graph.AddEdge(edge);
            /*edge = new DataEdge(vlist[0], vlist[2]);//{ SourceConnectionPointId = 3, TargetConnectionPointId = 1 };
            logicCore.Graph.AddEdge(edge);
            edge = new DataEdge(vlist[2], vlist[3]);
            logicCore.Graph.AddEdge(edge);
            edge = new DataEdge(vlist[2], vlist[4]);
            logicCore.Graph.AddEdge(edge);*/

            
            //edge = new DataEdge(vlist[1], vlist[2]) { SourceConnectionPointId = 3, TargetConnectionPointId = 2 };
            //logicCore.Graph.AddEdge(edge);

            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.EfficientSugiyama;
            logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.EfficientSugiyama);
            ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).Direction = LayoutDirection.RightToLeft;
            ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).EdgeRouting = SugiyamaEdgeRoutings.Orthogonal;
            ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).LayerDistance = 100;
            ((EfficientSugiyamaLayoutParameters) logicCore.DefaultLayoutAlgorithmParams).VertexDistance = 50;
            //logicCore.ExternalEdgeRoutingAlgorithm = new OrthEr<DataVertex, DataEdge, IMutableBidirectionalGraph<DataVertex, DataEdge>>(logicCore.Graph, null, null);

            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams = logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            logicCore.AsyncAlgorithmCompute = false;
            logicCore.EdgeCurvingEnabled = false;

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
               // dg_Area.VertexList[item].SetPosition(new Point());
            }

            //dg_Area.RelayoutGraph();
            //dg_zoomctrl.ZoomToFill();

            //dg_Area.UpdateAllEdges();
            //dg_Area.UpdateLayout();
            //dg_Area.InvalidateVisual();
            //dg_Area.RelayoutGraph();
            dg_zoomctrl.ZoomToFill();
        }

        public LogicCoreExample LogicCore { get; set; }

        private void LoadLogEntries()
        {
            dg_Area.LogicCoreChangeAction = LogicCoreChangedAction.GenerateGraphWithEdges;
            dg_Area.SetVerticesDrag(true, true);

            var core = new LogicCoreExample
            {
                Graph = ShowcaseHelper.GenerateDataGraph(2, false),
                DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER,
                DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK,
                DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA
            };
            var vlist = core.Graph.Vertices.ToList();
            var edge = new DataEdge(vlist[0], vlist[1]);
            core.Graph.AddEdge(edge);
            LogicCore = core;
            // This is the property that is bound to the GraphArea.
            OnPropertyChanged("LogicCore");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
