using System.ComponentModel;
using GraphX;
using System;
using YAXLib;

namespace ShowcaseExample
{
    [Serializable]
    public class DataEdge : EdgeBase<DataVertex>, INotifyPropertyChanged
    {
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
			: base(source, target, weight)
		{
            Angle = 90;
		}

        public DataEdge()
            : base(null, null, 1)
        {
            Angle = 90;
        }

        public double Angle { get; set; }

        /// <summary>
        /// Node main description (header)
        /// </summary>
        private string text;
        public string Text { get { return text; } set { text = value; OnPropertyChanged("Text"); } }
        public string ToolTipText {get; set; }

        public override string ToString()
        {
            return Text;
        }

        [YAXDontSerialize]
        public DataEdge Self
        {
            get { return this; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
