using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace GraphX.Controls
{
    [Bindable]
    public sealed class ZCP : ContentPresenter, INotifyPropertyChanged
    {
        public event ContentSizeChangedHandler ContentSizeChanged;

        private Size _contentSize;

       /* public Point ContentTopLeft { get; private set; }
        public Point ContentBottomRight { get; private set; }
        public Point ContentActualSize { get; private set; }
        */
        public Size ContentSize
        {
            get { return _contentSize; }
            private set {
                if (value == _contentSize)
                    return;

                _contentSize = value;
                if (ContentSizeChanged != null)
                    ContentSizeChanged(this, _contentSize);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var max = 1000000000;
            var x = double.IsInfinity(constraint.Width) ? max : constraint.Width;
            var y = double.IsInfinity(constraint.Height) ? max : constraint.Height;
            return new Size(x, y);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            UIElement child = Content != null
                                  ? VisualTreeHelper.GetChild(this, 0) as UIElement
                                  : null;
            if (child == null)
                return arrangeBounds;

            //set the ContentSize
            ContentSize = child.DesiredSize;
            child.Arrange(new Rect(new Point(),child.DesiredSize));
           /* if (child is GraphAreaBase)
            {
                ContentBottomRight = (child as GraphAreaBase).BottomRight;
                ContentTopLeft = (child as GraphAreaBase).TopLeft;
                ContentActualSize = new Point((child as GraphAreaBase).ActualWidth, (child as GraphAreaBase).ActualHeight);
            }*/

            return arrangeBounds;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
