using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GraphX.PCL.Common.Enums;
using ShowcaseApp.WPF.Annotations;
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
                Graph = ShowcaseHelper.GenerateDataGraph(4, false)
            };
            var vList = logicCore.Graph.Vertices.ToList();

            //add edges
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[0], vList[1]);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[1], vList[0]);
            ShowcaseHelper.AddEdge(logicCore.Graph, vList[2], vList[3]);

            graphArea.LogicCore = logicCore;
            //set positions 
            var posList = new Dictionary<DataVertex, Point>()
            {
                {vList[0], new Point(0, 0)},
                {vList[1], new Point(300, 0)},
                {vList[2], new Point(0, 300)},
                {vList[3], new Point(300, 300)},
            };

            //settings
            graphArea.LogicCore.EnableParallelEdges = true;
            graphArea.LogicCore.EdgeCurvingEnabled = true;
            graphArea.LogicCore.ParallelEdgeDistance = _edgeDistance;
            graphArea.LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            //preload graph
            graphArea.PreloadGraph(posList);
            //behaviors
            var eList = graphArea.EdgesList.Values.ToList();
            eList[0].LabelVerticalOffset = 22;
            eList[1].LabelVerticalOffset = 22;
            eList[2].LabelVerticalOffset = 22;

            graphArea.SetVerticesDrag(true, true);
            graphArea.ShowAllEdgesLabels();
            graphArea.AlignAllEdgesLabels();


            zoomControl.ZoomToFill();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
