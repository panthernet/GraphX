using GraphX.Controls;
using GraphX.GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using GraphX.Logic;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace WindowsFormsProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        void Form1_Load(object sender, EventArgs e)
        {
            wpfHost.Child = GenerateWpfVisuals();
            zoomctrl.ZoomToFill();
        }

        private ZoomControl zoomctrl;
        private GraphAreaExample gArea;

        private UIElement GenerateWpfVisuals()
        {
            zoomctrl = new ZoomControl();
            ZoomControl.SetViewFinderVisibility(zoomctrl, System.Windows.Visibility.Visible);
            /* ENABLES WINFORMS HOSTING MODE --- >*/
            var logic = new GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>();
            gArea = new GraphAreaExample() { EnableWinFormsHostingMode = true, LogicCore = logic };
            logic.Graph = GenerateGraph();
            logic.DefaultLayoutAlgorithm = GraphX.LayoutAlgorithmTypeEnum.KK;
            logic.DefaultLayoutAlgorithmParams = logic.AlgorithmFactory.CreateLayoutParameters(GraphX.LayoutAlgorithmTypeEnum.KK);
            ((KKLayoutParameters)logic.DefaultLayoutAlgorithmParams).MaxIterations = 100;
            logic.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            logic.DefaultOverlapRemovalAlgorithmParams = logic.AlgorithmFactory.CreateOverlapRemovalParameters(GraphX.OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;
            logic.DefaultEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.None;
            logic.AsyncAlgorithmCompute = false;
            zoomctrl.Content = gArea;
            gArea.RelayoutFinished += gArea_RelayoutFinished;

            var myResourceDictionary = new ResourceDictionary {Source = new Uri("Templates\\template.xaml", UriKind.Relative)};
            zoomctrl.Resources.MergedDictionaries.Add(myResourceDictionary);
            
            return zoomctrl;
        }

        void gArea_RelayoutFinished(object sender, EventArgs e)
        {
            zoomctrl.ZoomToFill();
        }

        private GraphExample GenerateGraph()
        {
            //FOR DETAILED EXPLANATION please see SimpleGraph example project
            var dataGraph = new GraphExample();
            for (int i = 1; i < 10; i++)
            {
                var dataVertex = new DataVertex("MyVertex " + i) { ID = i };
                dataGraph.AddVertex(dataVertex);
            }
            var vlist = dataGraph.Vertices.ToList();
            //Then create two edges optionaly defining Text property to show who are connected
            var dataEdge = new DataEdge(vlist[0], vlist[1]) { Text = string.Format("{0} -> {1}", vlist[0], vlist[1]) };
            dataGraph.AddEdge(dataEdge);
            dataEdge = new DataEdge(vlist[2], vlist[3]) { Text = string.Format("{0} -> {1}", vlist[2], vlist[3]) };
            dataGraph.AddEdge(dataEdge);

            dataEdge = new DataEdge(vlist[2], vlist[2]) { Text = string.Format("{0} -> {1}", vlist[2], vlist[2]) };
            dataGraph.AddEdge(dataEdge);
            return dataGraph;
        }

        private void but_generate_Click(object sender, EventArgs e)
        {
            gArea.GenerateGraph(true);
            gArea.SetVerticesDrag(true, true);
            zoomctrl.ZoomToFill();
        }

        private void but_reload_Click(object sender, EventArgs e)
        {
            gArea.RelayoutGraph();
        }
    }
}
