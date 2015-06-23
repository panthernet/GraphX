using System.ComponentModel;
using System.Windows.Controls;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for DebugGraph.xaml
    /// </summary>
    public partial class FeaturesPageEdges : UserControl, INotifyPropertyChanged
    {
        public FeaturesPageEdges()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
