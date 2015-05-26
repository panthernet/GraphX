using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
                if (CenterScale)
                    target.RenderTransformOrigin = new Point(.5, .5);
                else
                    target.RenderTransformOrigin = new Point(0, 0);
            }

            DoubleAnimation scaleAnimation =
                new DoubleAnimation(1, ScaleTo, new Duration(TimeSpan.FromSeconds(Duration)));

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        public void AnimateVertexBackward(VertexControl target)
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
                return; //no need to back cause default already
            }

            if (transform.ScaleX <= 1 || transform.ScaleY <= 1) return;

            DoubleAnimation scaleAnimation =
                new DoubleAnimation(transform.ScaleX, 1, new Duration(TimeSpan.FromSeconds(Duration)));

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
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
