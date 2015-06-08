using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GraphX.PCL.Common.Enums;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF.Pages.Mini
{
    /// <summary>
    /// Interaction logic for LayoutVCP.xaml
    /// </summary>
    public partial class LayoutVCP : UserControl
    {
        public LayoutVCP()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ControlLoaded;
            cbMathShape.Checked += CbMathShapeOnChecked;
            cbMathShape.Unchecked += CbMathShapeOnChecked;
        }

        private void CbMathShapeOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            foreach(var item in graphArea.VertexList.Values)
                item.VertexConnectionPointsList.ForEach(a => a.Shape = cbMathShape.IsChecked == true ? VertexShape.Circle : VertexShape.None);   
            graphArea.UpdateAllEdges(true);
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            GenerateGraph();
        }

        private void GenerateGraph()
        {
            var logicCore = new LogicCoreExample()
            {
                Graph = ShowcaseHelper.GenerateDataGraph(6, false)
            };
            var vList = logicCore.Graph.Vertices.ToList();

            //add edges
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[0], vList[1], 3, 2);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[0], vList[2], 4, 2);

            ShowcaseHelper.AddEdge(logicCore.Graph, vList[1], vList[3], 3, 1);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[3], vList[5], 2, 3);

            ShowcaseHelper.AddEdge(logicCore.Graph, vList[2], vList[4], 4, 2);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[4], vList[5], 1, 4);

            ShowcaseHelper.AddEdge(logicCore.Graph, vList[5], vList[1], 1, 4);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[5], vList[2], 2, 3);

            graphArea.LogicCore = logicCore;
            //set positions 
            var posList = new Dictionary<DataVertex, Point>()
            {
                {vList[0], new Point(0, 0)},
                {vList[1], new Point(200, -200)},
                {vList[2], new Point(200, 200)},
                {vList[3], new Point(600, -300)},
                {vList[4], new Point(600, 300)},
                {vList[5], new Point(400, 0)},
            };
            graphArea.PreloadGraph(posList);

            //settings
            graphArea.SetVerticesDrag(true, true);

            zoomControl.ZoomToFill();
        }
    }
}
