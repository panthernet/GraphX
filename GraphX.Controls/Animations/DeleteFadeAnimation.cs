using System;
#if WPF
using System.Windows;
using System.Windows.Media.Animation;
#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
#endif
using GraphX.Controls.Models;

namespace GraphX.Controls.Animations
{
    public sealed class DeleteFadeAnimation : IOneWayControlAnimation
    {
        public double Duration { get; set; }

        public DeleteFadeAnimation(double duration = .3)
        {
            Duration = duration;
        }

        private void RunAnimation(IGraphControl target, bool removeDataObject)
        {
            //create and run animation
            var story = new Storyboard();
            var fadeAnimation = new DoubleAnimation {Duration = new Duration(TimeSpan.FromSeconds(Duration)), FillBehavior = FillBehavior.Stop, From = 1, To = 0};
            fadeAnimation.SetDesiredFrameRate(30);
            fadeAnimation.Completed += (sender, e) => OnCompleted(target, removeDataObject);
            story.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, target as FrameworkElement);
#if WPF
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            story.Begin(target as FrameworkElement);            
#elif METRO
            Storyboard.SetTargetProperty(fadeAnimation, "Opacity");            
            story.Begin();
#else
            throw new NotImplementedException();
#endif
        }

        public void AnimateVertex(VertexControl target, bool removeDataVertex = false)
        {
            RunAnimation(target, removeDataVertex);
        }

        public void AnimateEdge(EdgeControl target, bool removeDataEdge = false)
        {
            RunAnimation(target, removeDataEdge);
        }

        public event RemoveControlEventHandler Completed;

        public void OnCompleted(IGraphControl target, bool removeDataObject)
        {
            if (Completed != null)
                Completed(this, new ControlEventArgs(target, removeDataObject));
        }
    }
}
