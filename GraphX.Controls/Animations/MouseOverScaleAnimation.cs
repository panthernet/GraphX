using System;
#if WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
#elif METRO
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#endif

namespace GraphX.Controls.Animations
{
    public sealed class MouseOverScaleAnimation : IBidirectionalControlAnimation
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

        public MouseOverScaleAnimation(double duration = .3, double scaleto = 1.2, bool centerscale = true)
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
                target.RenderTransformOrigin = CenterScale ? new Point(.5, .5) : new Point(0, 0);
            }

#if WPF
            var scaleAnimation =new DoubleAnimation(1, ScaleTo, new Duration(TimeSpan.FromSeconds(Duration)));
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
#elif METRO
            var sb = new Storyboard();
            var scaleAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(Duration)), From = 1, To = ScaleTo };
            Storyboard.SetTarget(scaleAnimation, target);
            Storyboard.SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            sb.Children.Add(scaleAnimation);
            scaleAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(Duration)), From = 1, To = ScaleTo };
            Storyboard.SetTarget(scaleAnimation, target);
            Storyboard.SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            sb.Children.Add(scaleAnimation);
            sb.Begin();
#else
            throw new NotImplementedException();
#endif

        }

        public void AnimateVertexBackward(VertexControl target)
        {
            var transform = CustomHelper.GetScaleTransform(target);
            if (transform == null)
            {
                target.RenderTransform = new ScaleTransform();
                target.RenderTransformOrigin = CenterScale ? new Point(.5, .5) : new Point(0, 0);
                return; //no need to back cause default already
            }

            if (transform.ScaleX <= 1 || transform.ScaleY <= 1) return;

#if WPF
            var scaleAnimation = new DoubleAnimation(transform.ScaleX, 1, new Duration(TimeSpan.FromSeconds(Duration)));
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
#elif METRO
            var sb = new Storyboard();
            var scaleAnimation = new DoubleAnimation{ Duration = new Duration(TimeSpan.FromSeconds(Duration)), From = transform.ScaleX, To = 1 };
            Storyboard.SetTarget(scaleAnimation, target);
            Storyboard.SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            sb.Children.Add(scaleAnimation);
            scaleAnimation = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(Duration)), From = transform.ScaleX, To = 1 };
            Storyboard.SetTarget(scaleAnimation, target);
            Storyboard.SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            sb.Children.Add(scaleAnimation);
            sb.Begin();
#else
            throw new NotImplementedException();
#endif
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
