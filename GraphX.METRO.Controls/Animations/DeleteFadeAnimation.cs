using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using GraphX.Controls;
using System;
using GraphX.METRO.Controls.Models.Interfaces;

namespace GraphX.Models.Animations
{
    public sealed class DeleteFadeAnimation : IOneWayControlAnimation
    {
        public double Duration { get; set; }

        public DeleteFadeAnimation(double duration = .3)
        {
            Duration = duration;
        }

        private void RunAnimation(IGraphControl target)
        {
            //create and run animation
            var story = new Storyboard();
            var fadeAnimation = new DoubleAnimation {Duration = new Duration(TimeSpan.FromSeconds(Duration)), FillBehavior = FillBehavior.Stop, From = 1, To = 0};
            fadeAnimation.Completed += (sender, e) => OnCompleted(target);
            story.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, target as FrameworkElement);
            Storyboard.SetTargetProperty(fadeAnimation, "Opacity");            
            story.Begin();
        }

        public void AnimateVertex(VertexControl target)
        {
            RunAnimation(target);
        }

        public void AnimateEdge(EdgeControl target)
        {
            RunAnimation(target);
        }

        public event RemoveControlEventHandler Completed;

        public void OnCompleted(IGraphControl target)
        {
            if (Completed != null)
                Completed(this, new ControlEventArgs(target));
        }
    }
}
