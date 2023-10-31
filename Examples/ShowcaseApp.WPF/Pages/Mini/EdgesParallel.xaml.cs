using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GraphX.Controls;
using GraphX.Controls.Models;
using GraphX.Common.Enums;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF.Pages.Mini
{
    /// <summary>
    /// Interaction logic for LayoutVCP.xaml
    /// </summary>
    public partial class EdgesParallel : UserControl, INotifyPropertyChanged
    {
        private int _edgeDistance;
        public int EdgeDistance { get { return _edgeDistance; } 
            set 
            { 
                _edgeDistance = value;
                graphArea.LogicCore.ParallelEdgeDistance = value; 
                graphArea.UpdateAllEdges(true);
                OnPropertyChanged("EdgeDistance"); 
            } }

        public EdgesParallel()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ControlLoaded;
        
            cbEnablePE.IsChecked = true;
            _edgeDistance = 10;

            cbEnablePE.Checked += CbMathShapeOnChecked;
            cbEnablePE.Unchecked += CbMathShapeOnChecked;
            graphArea.EdgeLabelFactory = new DefaultEdgelabelFactory();
        }

        private void CbMathShapeOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            graphArea.LogicCore.EnableParallelEdges = (bool) cbEnablePE.IsChecked;
            graphArea.UpdateAllEdges(true);
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged("EdgeDistance");
            GenerateGraph();
        }

        private void GenerateGraph()
        {
            var logicCore = new LogicCoreExample()
            {
                Graph = ShowcaseHelper.GenerateDataGraph(3, false)
            };
            var vList = logicCore.Graph.Vertices.ToList();

            //add edges
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[0], vList[1]);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[1], vList[0]);

            ShowcaseHelper.AddEdge(logicCore.Graph, vList[1], vList[2]);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[1], vList[2]);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[2], vList[1]);

            graphArea.LogicCore = logicCore;
            //set positions 
            var posList = new Dictionary<DataVertex, Point>()
            {
                {vList[0], new Point(0, -150)},
                {vList[1], new Point(300, 0)},
                {vList[2], new Point(600, -150)},
            };

            //settings
            graphArea.LogicCore.EnableParallelEdges = true;
            graphArea.LogicCore.EdgeCurvingEnabled = true;
            graphArea.LogicCore.ParallelEdgeDistance = _edgeDistance;
            graphArea.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            //preload graph
            graphArea.PreloadGraph(posList);
            //behaviors
            graphArea.SetVerticesDrag(true, true);
            graphArea.ShowAllEdgesLabels();
            graphArea.AlignAllEdgesLabels();
            zoomControl.MaxZoom = 50;
            //manual edge corrections
            var eList = graphArea.EdgesList.Values.ToList();
            eList[0].GetLabelControls().FirstOrDefault().LabelVerticalOffset = 12;
            eList[1].GetLabelControls().FirstOrDefault().LabelVerticalOffset = 12;

            eList[2].GetLabelControls().FirstOrDefault().ShowLabel = false;
           // eList[3].GetLabelControls().FirstOrDefault().LabelVerticalOffset = 12;
           // eList[4].GetLabelControls().FirstOrDefault().LabelVerticalOffset = -12;

            //PS: to see how parallel edges logic works go to GraphArea::UpdateParallelEdgesData() method

            zoomControl.ZoomToFill();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
