using GraphX.Models;
using ShowcaseExample.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ShowcaseExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Properties
        /// <summary>
        /// General data source
        /// </summary>
        public List<DataItem> DataSource { get; set; }
        /// <summary>
        /// Random generator
        /// </summary>
        private Random Rand = new Random();
        #endregion

        private const int datasourceSize = 5000;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            GeneralGraph_Constructor();
            ThemedGraph_Constructor();
            ERGraph_Constructor();
            DynamicGraph_Constructor();
            TestGround_Constructor();
            DataSource = GenerateData(datasourceSize);
            Closed += MainWindow_Closed;            
#if RELEASE
            tabctrl.Items.Remove(tgtab);
#endif
            Loaded += MainWindow_Loaded;
        }


        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
           // MessageBox.Show("Currently this showcase app doesn't include all GraphX features. For the latest info look in the changelog.txt file and Documentation section on the web site! Thanks!",
           //     "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region GenerateData
        /// <summary>
        /// Generate sample data
        /// </summary>
        /// <param name="count">Data items count</param>
        private List<DataItem> GenerateData(int count)
        {
            var list = new List<DataItem>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new DataItem() { ID = i, Text = Rand.Next(100000).ToString() });
            }
            return list;
        }
        #endregion

        #region GenerateDataGraph
        /// <summary>
        /// Generate example graph data
        /// </summary>
        /// <param name="count">Items count</param>
        private GraphExample GenerateDataGraph(int count)
        {
            var graph = new GraphExample();

            foreach (var item in DataSource.Take(count))
                graph.AddVertex(new DataVertex(item.Text) { ID = item.ID });

            var vlist = graph.Vertices.ToList();

            /*graph.AddEdge(new DataEdge(vlist[0], vlist[3], 1));
            graph.AddEdge(new DataEdge(vlist[1], vlist[4], 1));
            graph.AddEdge(new DataEdge(vlist[2], vlist[5], 1));*/
            var cnt = 1;
            foreach (var item in vlist)
            {
                if (Rand.Next(0, 50) > 25) continue;
                var vertex2 = vlist[Rand.Next(0, graph.VertexCount - 1)];
                var txt = string.Format("{0} -> {1}", item.Text, vertex2.Text);
                graph.AddEdge(new DataEdge(item, vertex2, Rand.Next(1, 50)) { ID = cnt, Text = txt, ToolTipText = txt });
                cnt++;
            }
            return graph;
        }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if(PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void erg_toggleVertex_Click(object sender, RoutedEventArgs e)
        {
            if (erg_Area.VertexList.First().Value.Visibility == System.Windows.Visibility.Visible)
                foreach (var item in erg_Area.VertexList)
                    item.Value.Visibility = System.Windows.Visibility.Collapsed;
            else foreach (var item in erg_Area.VertexList)
                    item.Value.Visibility = System.Windows.Visibility.Visible;

        }

        private void but_about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coded by Alexander 'PantheR' Smirnov on 2013 - 2014\nWeb: http://panthernet.ru/en/projects-en/graphx-en", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }










    }
}
