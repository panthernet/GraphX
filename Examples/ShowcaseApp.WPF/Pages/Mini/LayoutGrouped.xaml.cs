using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphX;
using GraphX.PCL.Common;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms.Grouped;
using QuickGraph;
using ShowcaseApp.WPF.Models;
using Rect = GraphX.Measure.Rect;

namespace ShowcaseApp.WPF.Pages.Mini
{
    /// <summary>
    /// Interaction logic for LayoutVCP.xaml
    /// </summary>
    public partial class LayoutGrouped : UserControl
    {
        public LayoutGrouped()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ControlLoaded;
            graphArea.SideExpansionSize = new Size(80, 80);
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            GenerateGraph();
        }

        private void GenerateGraph()
        {
            graphArea.ClearLayout();
            var logicCore = new LogicCoreExample()
            {
                Graph = ShowcaseHelper.GenerateDataGraph(10)
            };
            logicCore.Graph.Vertices.Take(5).ForEach(a => a.GroupId = 1);
            logicCore.Graph.Vertices.Where(a => a.GroupId == 0).ForEach(a => a.GroupId = 2);
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.None;
            //generate group params
            var prms = new List<AlgorithmGroupParameters<DataVertex, DataEdge>>
            {
                new AlgorithmGroupParameters<DataVertex, DataEdge>
                {
                    GroupId = 1,
                    LayoutAlgorithm =
                        new RandomLayoutAlgorithm<DataVertex, DataEdge, GraphExample>(
                            new RandomLayoutAlgorithmParams {Bounds = new Rect(10, 10, 490, 490)}),
                    
                },
                new AlgorithmGroupParameters<DataVertex, DataEdge>
                {
                    GroupId = 2,
                    LayoutAlgorithm =
                        new RandomLayoutAlgorithm<DataVertex, DataEdge, GraphExample>(
                            new RandomLayoutAlgorithmParams {Bounds = new Rect(10, 10, 490, 490)}),
                }
            };

            var gParams = new GroupingLayoutAlgorithmParameters<DataVertex, DataEdge>(prms, true)
            {
                OverlapRemovalAlgorithm = logicCore.AlgorithmFactory.CreateFSAA<object>(100, 100),
                ArrangeGroups = cbArrangeGroups.IsChecked ?? false,
            };
            //generate grouping algo
            logicCore.ExternalLayoutAlgorithm =
                new GroupingLayoutAlgorithm<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>(logicCore.Graph, null, gParams);

            graphArea.LogicCore = logicCore;
            //generate graphs
            graphArea.GenerateGraph();

            //generate group visuals
            foreach (var item in prms)
            {
                if (!item.ZoneRectangle.HasValue) continue;
                var rect = GenerateGroupBorder(item);
                graphArea.InsertCustomChildControl(0, rect);
                GraphAreaBase.SetX(rect, item.ZoneRectangle.Value.X - _groupInnerPadding *.5);
                GraphAreaBase.SetY(rect, item.ZoneRectangle.Value.Y - _groupInnerPadding *.5 - _headerHeight);
            }
            zoomControl.ZoomToFill();
        }

        private double _headerHeight = 30;
        private double _groupInnerPadding = 20;

        private Border GenerateGroupBorder(AlgorithmGroupParameters<DataVertex, DataEdge> prms)
        {
            return new Border
            {
                Width = prms.ZoneRectangle.Value.Width + _groupInnerPadding,
                Height = prms.ZoneRectangle.Value.Height + _groupInnerPadding + _headerHeight,
                Background = prms.GroupId == 1 ? Brushes.LightSkyBlue : Brushes.Gray,
                Opacity = 1,
                CornerRadius = new CornerRadius(8),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Child = new Border
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    CornerRadius = new CornerRadius(8, 8, 0, 0),
                    Background = Brushes.Black,
                    Height = _headerHeight,
                    Child = new TextBlock
                    {
                        Text = string.Format("Group {0}", prms.GroupId),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        FontSize = 12
                    }
                }
            };            
        }

        private void GraphRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            GenerateGraph();
        }
    }
}
