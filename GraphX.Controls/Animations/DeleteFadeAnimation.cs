using GraphX.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

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
            var fadeAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(Duration)), FillBehavior.Stop);
            fadeAnimation.Completed += (sender, e) => { OnCompleted(target); };
            story.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, target as FrameworkElement);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            story.Begin(target as FrameworkElement);
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
