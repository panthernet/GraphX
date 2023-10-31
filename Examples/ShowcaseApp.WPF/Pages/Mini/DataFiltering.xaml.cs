using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GraphX.Common.Enums;
using ShowcaseApp.WPF.Filters;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF.Pages.Mini
{
    /// <summary>
    /// Interaction logic for LayoutVCP.xaml
    /// </summary>
    public partial class DataFiltering : UserControl, INotifyPropertyChanged
    {

        public DataFiltering()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ControlLoaded;

            cboxFilter.ItemsSource = new List<string> {"None", "Only yellow vertices", "Only blue vertices"};
            cboxFilter.SelectedIndex = 0;
            cboxFilter.SelectionChanged += CboxFilter_SelectionChanged;
        }

        private void CboxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            graphArea.LogicCore.Filters.Clear();
            if (cboxFilter.SelectedIndex == 1)
                graphArea.LogicCore.Filters.Enqueue(new YellowVertexFilter());
            else if(cboxFilter.SelectedIndex == 2)
                graphArea.LogicCore.Filters.Enqueue(new BlueVertexFilter());
            graphArea.RelayoutGraph();

        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            GenerateGraph();
        }

        private void GenerateGraph()
        {
            graphArea.LogicCore = new LogicCoreExample
            {
                Graph = ShowcaseHelper.GenerateDataGraph(30),
                EdgeCurvingEnabled = true,
                DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER,
                DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.SimpleRandom
            };
            graphArea.LogicCore.Graph.Vertices.ToList().ForEach(a=> a.IsBlue = Convert.ToBoolean(ShowcaseHelper.Rand.Next(2)));

            //settings
            //gen graph
            graphArea.GenerateGraph();
            //behaviors
            graphArea.SetVerticesDrag(true, true);
            zoomControl.MaxZoom = 50;
            zoomControl.ZoomToFill();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
