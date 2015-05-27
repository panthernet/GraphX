using System;
#if WPF
using System.Windows.Controls;
using System.Windows.Media.Animation;
#elif METRO
using Windows.UI.Xaml.Controls;
using GraphX.Controls.Models;
#endif

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
                var control = item.Key as Control;
                var from = GraphAreaBase.GetX(control);
                from = double.IsNaN(from) ? 0.0 : from;

#if WPF
                //create the animation for the horizontal position
                var animationX = new DoubleAnimation(
                    from,
                    item.Value.X,
                    Duration,
                    FillBehavior.HoldEnd);
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
                animationY.Completed += (s, e) =>
                {
                    control.BeginAnimation(GraphAreaBase.YProperty, null);
                    control.SetValue(GraphAreaBase.YProperty, item.Value.Y);
                    _counter++;
                    if (_counter == _maxCount) OnCompleted();
                };
                control.BeginAnimation(GraphAreaBase.YProperty, animationY, HandoffBehavior.Compose);
#elif METRO
                var to = item.Value.X;

                //Here we implement workaround for WinRT stupid limitations:
                // - Can't animate custom attached props

                //First we set final coordinate that doesn't affect rendering
                // --> Already set in GraphArea computation logic <-- GraphAreaBase.SetFinalX(control, to);
                //And now we animate Canvas.Left property that affect rendering
                AnimationHelper.CreateDoubleAnimation(from, to, Duration.TotalMilliseconds, "(Canvas.Left)", control, null,
                    (s, e) =>
                    {
                        //After animation is complete we set X coordinate to FinalX stored earlier
                        //This is needed to maintain old coordinates system and avoid major changes in overall library
                        GraphAreaBase.SetX(control, GraphAreaBase.GetFinalX(control));
                        _counter++;
                        if (_counter == _maxCount) OnCompleted();
                    }).Begin();
                

                //Repeat the same for Y coordinates
                from = GraphAreaBase.GetY(control);
                from = (double.IsNaN(from) ? 0.0 : from);
                to = item.Value.Y;
                //GraphAreaBase.SetFinalY(control, to);
                AnimationHelper.CreateDoubleAnimation(from, to, Duration.TotalMilliseconds, "(Canvas.Top)", control, null,
                   (s, e) =>
                   {
                       GraphAreaBase.SetY(control, GraphAreaBase.GetFinalY(control));
                       _counter++;
                       if (_counter == _maxCount) OnCompleted();
                   }).Begin();
#else
            throw new NotImplementedException();
#endif
            }
        }
    }
}