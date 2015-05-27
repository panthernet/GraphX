using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using GraphX;
using System;
using GraphX.Controls;
using GraphX.Controls.Animations;

namespace METRO.SimpleGraph
{
    public sealed class MouseOverAnimation : IBidirectionalControlAnimation
    {
        /// <summary>
        /// Scale to this value. Default size is 1. For ex. 2 will double the size of the object.
        /// </summary>
        public double ScaleTo { get; set; }
        /// <summary>
        /// Scale from the center of the object or from the left top corner
        /// </summary>
        public bool CenterScale { get; set; }

        /// <summary>
        /// Animation duration
        /// </summary>
        public double Duration { get; set; }

        public MouseOverAnimation(double duration = .3, double scaleto = 1.2, bool centerscale = true)
        {
            Duration = duration;
            ScaleTo = scaleto;
            CenterScale = centerscale;
        }

        public void AnimateVertexForward(VertexControl target)
        {
            var transform = CustomHelper.GetScaleTransform(target);
            if (transform == null)
            {
                target.RenderTransform = new ScaleTransform();
                transform = target.RenderTransform as ScaleTransform;
                if (CenterScale)
                    target.RenderTransformOrigin = new Point(.5, .5);
                else
                    target.RenderTransformOrigin = new Point(0, 0);
            }

            var sb = new Storyboard();
            var scaleAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(Duration)), From = 1, To = ScaleTo };
            Storyboard.SetTarget(scaleAnimation, target);
            Storyboard.SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            sb.Children.Add(scaleAnimation);
            scaleAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(Duration)), From = 1, To = ScaleTo };
            Storyboard.SetTarget(scaleAnimation, target);
            Storyboard.SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            sb.Children.Add(scaleAnimation);

            //transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            //transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            sb.Begin();
        }

        public void AnimateVertexBackward(VertexControl target)
        {
            var transform = CustomHelper.GetScaleTransform(target);
            if (transform == null)
            {
                target.RenderTransform = new ScaleTransform();
                transform = target.RenderTransform as ScaleTransform;
                target.RenderTransformOrigin = CenterScale ? new Point(.5, .5) : new Point(0, 0);
                return; //no need to back cause default already
            }

            if (transform.ScaleX <= 1 || transform.ScaleY <= 1) return;

            var sb = new Storyboard();
            var scaleAnimation = new DoubleAnimation{ Duration = new Duration(TimeSpan.FromSeconds(Duration)), From = transform.ScaleX, To = 1 };
            Storyboard.SetTarget(scaleAnimation, target);
            Storyboard.SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            sb.Children.Add(scaleAnimation);
            scaleAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(Duration)), From = transform.ScaleX, To = 1 };
            Storyboard.SetTarget(scaleAnimation, target);
            Storyboard.SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            sb.Children.Add(scaleAnimation);

            //transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            //transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            sb.Begin();
        }

        public void AnimateEdgeForward(EdgeControl target)
        {
            //not implemented
        }

        public void AnimateEdgeBackward(EdgeControl target)
        {
            //not implemented
        }
    }
}
