using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphX;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using QuickGraph;
using ShowcaseApp.WPF.Models;
using Rect = GraphX.Measure.Rect;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for DebugGraph.xaml
    /// </summary>
    public partial class DebugGraph : UserControl, INotifyPropertyChanged
    {
        private DebugModeEnum _debugMode;
        public DebugModeEnum DebugMode { get { return _debugMode; } set { _debugMode = value; OnPropertyChanged("DebugMode"); } }

        public DebugGraph()
        {
            InitializeComponent();
            DataContext = this;
            butEdgePointer.Click += butEdgePointer_Click;
            butGeneral.Click += butGeneral_Click;
            butRelayout.Click += butRelayout_Click;
            butVCP.Click += butVCP_Click;
            butEdgeLabels.Click += butEdgeLabels_Click;
            butGroupedGraph.Click += butGroupedGraph_Click;
            cbDebugMode.ItemsSource = Enum.GetValues(typeof(DebugModeEnum)).Cast<DebugModeEnum>();
            cbDebugMode.SelectionChanged += cbDebugMode_SelectionChanged;
            dg_zoomctrl.PropertyChanged += dg_zoomctrl_PropertyChanged;
            CreateNewArea();
            dg_zoomctrl.MaximumZoomStep = 100;
        }

        void butGroupedGraph_Click(object sender, RoutedEventArgs e)
        {
            CreateNewArea();
            dg_Area.LogicCore.Graph = ShowcaseHelper.GenerateDataGraph(10, true);
            dg_Area.LogicCore.Graph.Vertices.Take(5).ForEach(a => a.GroupId = 1);
            dg_Area.LogicCore.Graph.Vertices.Where(a=> a.GroupId == 0).ForEach(a => a.GroupId = 2);
            dg_Area.LogicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.None;
            //generate group params
            var prms = new List<AlgorithmGroupParameters<DataVertex>>()
            {
                new AlgorithmGroupParameters<DataVertex>()
                {
                    GroupId = 1,
                    LayoutAlgorithm =
                        new RandomLayoutAlgorithm<DataVertex, DataEdge, IVertexAndEdgeListGraph<DataVertex, DataEdge>>(
                            dg_Area.LogicCore.Graph, null,
                            new RandomLayoutAlgorithmParams {Bounds = new Rect(0, 0, 500, 500)}),
                    ZoneRectangle = new Rect(0, 0, 500, 500)
                },
                new AlgorithmGroupParameters<DataVertex>()
                {
                    GroupId = 2,
                    LayoutAlgorithm =
                        new RandomLayoutAlgorithm<DataVertex, DataEdge, IVertexAndEdgeListGraph<DataVertex, DataEdge>>(
                            dg_Area.LogicCore.Graph, null,
                            new RandomLayoutAlgorithmParams {Bounds = new Rect(1000, 0, 500, 500)}),
                    ZoneRectangle = new Rect(1000, 0, 500, 500)
                }
            };
            //generate grouping algo
            dg_Area.LogicCore.ExternalLayoutAlgorithm =
                new GroupingLayoutAlgorithm<DataVertex, DataEdge, IVertexAndEdgeListGraph<DataVertex, DataEdge>>(
                    dg_Area.LogicCore.Graph, null, prms);

            //generate graphs
            dg_Area.GenerateGraph();

            //generate group visuals
            foreach (var item in prms)
            {
                var rect = new Rectangle()
                {
                    Width = item.ZoneRectangle.Width,
                    Height = item.ZoneRectangle.Height,
                    Fill = item.GroupId == 1 ? Brushes.Blue : Brushes.Black,
                    Opacity = .5
                };
                dg_Area.InsertCustomChildControl(0, rect);
                GraphAreaBase.SetX(rect, item.ZoneRectangle.X);
                GraphAreaBase.SetY(rect, item.ZoneRectangle.Y);
            }
            dg_zoomctrl.ZoomToFill();
        }

        void dg_zoomctrl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Zoom")
            {
                Debug.WriteLine("Zoom: "+ dg_zoomctrl.Zoom);
            }
        }

        void CreateNewArea()
        {
            dg_Area.Dispose();
            dg_Area = new GraphAreaExample() {Name = "dg_Area", LogicCore = new LogicCoreExample()};
            dg_Area.Resources = new ResourceDictionary() { Source = new Uri("/Templates/Debug/TestTemplates.xaml", UriKind.RelativeOrAbsolute) };
            dg_Area.SetVerticesDrag(true, true);
            dg_Area.GenerateGraphFinished += dg_Area_GenerateGraphFinished;
            dg_Area.RelayoutFinished += dg_Area_GenerateGraphFinished;
            dg_zoomctrl.Content = dg_Area;
            dg_Area.ShowAllEdgesLabels(false);

        }

        void dg_Area_GenerateGraphFinished(object sender, EventArgs e)
        {
            dg_zoomctrl.ZoomToFill();
        }

        void butEdgeLabels_Click(object sender, RoutedEventArgs e)
        {
            CreateNewArea();

            dg_Area.ShowAllEdgesLabels(true);
            dg_Area.AlignAllEdgesLabels(true);
            dg_Area.ShowAllEdgesArrows(true);

            dg_Area.LogicCore.Graph = ShowcaseHelper.GenerateDataGraph(2, false);

            var vlist = dg_Area.LogicCore.Graph.Vertices.ToList();
            var edge = new DataEdge(vlist[0], vlist[1]) {Text = "Testing edge labels..."};
            dg_Area.LogicCore.Graph.AddEdge(edge);

            dg_Area.PreloadGraph(new Dictionary<DataVertex, Point>() { {vlist[0], new Point()}, {vlist[1], new Point(0, 200)}});
            dg_Area.VertexList.Values.ToList().ForEach(a => a.SetConnectionPointsVisibility(false));
        }

        void butVCP_Click(object sender, RoutedEventArgs e)
        {
            CreateNewArea();
            dg_Area.VertexList.Values.ToList().ForEach(a=> a.SetConnectionPointsVisibility(true));
            dg_Area.LogicCore.Graph = ShowcaseHelper.GenerateDataGraph(4, false);
            var vlist = dg_Area.LogicCore.Graph.Vertices.ToList();
            var edge = new DataEdge(vlist[0], vlist[1]) { SourceConnectionPointId = 1, TargetConnectionPointId = 1 };
            dg_Area.LogicCore.Graph.AddEdge(edge);
            edge = new DataEdge(vlist[0], vlist[2]) { SourceConnectionPointId = 3, TargetConnectionPointId = 1 };
            dg_Area.LogicCore.Graph.AddEdge(edge);

            dg_Area.LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            dg_Area.LogicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            dg_Area.LogicCore.DefaultOverlapRemovalAlgorithmParams = dg_Area.LogicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)dg_Area.LogicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)dg_Area.LogicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;
            dg_Area.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;

            dg_Area.GenerateGraph(true);

            var vertex = dg_Area.VertexList[edge.Target];
            var vcp = vertex.VertexConnectionPointsList.First();
            var newVcp = new StaticVertexConnectionPoint() {Id = 5};
            ((StackPanel)((Border) vcp.GetParent()).Parent).Children.Add(newVcp);
            edge.TargetConnectionPointId = 5;
            vertex.VertexConnectionPointsList.Add(newVcp);
            dg_Area.EdgesList[edge].UpdateEdge();
            //dg_Area.UpdateAllEdges(true);
        }

        void butRelayout_Click(object sender, RoutedEventArgs e)
        {
            dg_Area.RelayoutGraph(true);    
        }

        void butGeneral_Click(object sender, RoutedEventArgs e)
        {
            CreateNewArea();
            dg_Area.LogicCore.Graph = ShowcaseHelper.GenerateDataGraph(2, false);
            var vlist = dg_Area.LogicCore.Graph.Vertices.ToList();
            dg_Area.LogicCore.Graph.AddEdge(new DataEdge(vlist[0], vlist[1]) { ArrowTarget = true});
           // dg_Area.LogicCore.Graph.AddEdge(new DataEdge(vlist[1], vlist[2]) { ArrowTarget = false});
            dg_Area.LogicCore.EdgeCurvingEnabled = true;
            //dg_Area.PreloadGraph(new Dictionary<DataVertex, Point>() { {vlist[0], new Point()} });
            dg_Area.LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            dg_Area.GenerateGraph(true);
            
            dg_Area.VertexList.Values.ToList().ForEach(a => a.SetConnectionPointsVisibility(false));
        }

        void butEdgePointer_Click(object sender, RoutedEventArgs e)
        {
            CreateNewArea();
            dg_Area.VertexList.Values.ToList().ForEach(a => a.SetConnectionPointsVisibility(false));
        }

        #region DebugMode switches
        void cbDebugMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DebugMode)
            {
                case DebugModeEnum.Clean:
                    CleanDMAnimations();
                    CleanDMERCurving();
                    CleanDMER();
                    break;
                case DebugModeEnum.Animations:
                    CleanDMERCurving();
                    CleanDMER();
                    dg_Area.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(0.5));
                    dg_Area.MoveAnimation.Completed += dg_Area_GenerateGraphFinished;
                    dg_Area.MouseOverAnimation = AnimationFactory.CreateMouseOverAnimation(MouseOverAnimation.Scale);
                    dg_Area.DeleteAnimation = AnimationFactory.CreateDeleteAnimation(DeleteAnimation.Fade);
                    break;
                case DebugModeEnum.EdgeRoutingEnabled:
                    CleanDMAnimations();
                    CleanDMERCurving();
                    dg_Area.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
                    break;
                case DebugModeEnum.EdgeRoutingWithCurvingEnabled:
                    CleanDMAnimations();
                    CleanDMER();
                    dg_Area.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
                    dg_Area.LogicCore.EdgeCurvingEnabled = true;
                    break;
            }
        }

        private void CleanDMERCurving()
        {
            dg_Area.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            dg_Area.LogicCore.EdgeCurvingEnabled = false;
        }

        private void CleanDMER()
        {
            dg_Area.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
        }

        private void CleanDMAnimations()
        {
            if (dg_Area.MoveAnimation != null)
                dg_Area.MoveAnimation.Completed -= dg_Area_GenerateGraphFinished;
            dg_Area.MoveAnimation = null;
            dg_Area.MouseOverAnimation = null;
            dg_Area.DeleteAnimation = null;
        }
        
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public enum DebugModeEnum
    {
        Clean,
        Animations,
        EdgeRoutingEnabled,
        EdgeRoutingWithCurvingEnabled
    }
}
