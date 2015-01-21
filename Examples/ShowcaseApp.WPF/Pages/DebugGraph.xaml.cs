using System.Windows;
using System.Windows.Controls;

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
        }

        void butTest_Click(object sender, RoutedEventArgs e)
        {
            var win = new Window {Content = new EditorGraph()};
            win.ShowDialog();
        }

        void butRun_Click(object sender, RoutedEventArgs e)
        {
            var lc = new LogicCoreExample() {Graph = new GraphExample()};
            lc.Graph.AddVertex(new DataVertex("Test vertex"));
            dg_Area.LogicCore = lc;
            dg_Area.GenerateGraph(true);
        }
    }
}
