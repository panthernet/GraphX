using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace GraphX.Controls.Models
{
    public static class AnimationHelper
    {
        public static Storyboard CreateDoubleAnimation(double? from, double? to, double duration, string propertyName, FrameworkElement target, FillBehavior? fillBehavior = null, EventHandler<object> onCompleted = null)
        {
            var animation = new DoubleAnimation 
            { 
                From = from, 
                To = to, 
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)), 
                EnableDependentAnimation = true ,
                //FillBehavior = fillBehavior
                //EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };
            
            if (fillBehavior.HasValue)
                animation.FillBehavior = fillBehavior.Value;
            var sb = new Storyboard();
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, propertyName);
            sb.Children.Add(animation);
            if(onCompleted != null)
                sb.Completed += onCompleted;
            return sb;
        }
    }

}
