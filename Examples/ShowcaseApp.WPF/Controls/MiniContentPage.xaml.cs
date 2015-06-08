using System.ComponentModel;
using System.Windows.Controls;
using ShowcaseApp.WPF.Controls;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for MiniContentPage.xaml
    /// </summary>
    public partial class MiniContentPage : UserControl, ISpecialWindowContentIntro, INotifyPropertyChanged
    {
        public MiniContentPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private string _text;
        public string IntroText { get { return _text; } set { _text = value;  OnPropertyChanged("IntroText"); } }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
