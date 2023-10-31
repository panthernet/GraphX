using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GraphX.Controls.Animations
{
    public sealed class MoveSimpleAnimation : MoveAnimationBase
    {
        public MoveSimpleAnimation(TimeSpan duration)
        {
            Duration = duration;
        }

        int _maxCount;
        int _counter;

        public override void Cleanup()
        {
        }

        public override void RunVertexAnimation()
        {
            _maxCount = VertexStorage.Count * 2;
            _counter = 0;
            foreach (var item in VertexStorage)
            {
                var control = (Control) item.Key;
                var from = GraphAreaBase.GetX(control);
                from = double.IsNaN(from) ? 0.0 : from;

                //create the animation for the horizontal position
                var animationX = new DoubleAnimation(
                    from,
                    item.Value.X,
                    Duration,
                    FillBehavior.HoldEnd);
                Timeline.SetDesiredFrameRate(animationX, 30);
                animationX.Completed += (s, e) =>
                {
                    control.BeginAnimation(GraphAreaBase.XProperty, null);
                    control.SetValue(GraphAreaBase.XProperty, item.Value.X);
                    _counter++;
                    if (_counter == _maxCount) OnCompleted();
                };
                control.BeginAnimation(GraphAreaBase.XProperty, animationX, HandoffBehavior.Compose);


                from = GraphAreaBase.GetY(control);
                from = (double.IsNaN(from) ? 0.0 : from);

                //create an animation for the vertical position
                var animationY = new DoubleAnimation(
                    from, item.Value.Y,
                    Duration,
                    FillBehavior.HoldEnd);
                Timeline.SetDesiredFrameRate(animationY, 30);
                animationY.Completed += (s, e) =>
                {
                    control.BeginAnimation(GraphAreaBase.YProperty, null);
                    control.SetValue(GraphAreaBase.YProperty, item.Value.Y);
                    _counter++;
                    if (_counter == _maxCount) OnCompleted();
                };
                control.BeginAnimation(GraphAreaBase.YProperty, animationY, HandoffBehavior.Compose);
            }
        }
    }
}