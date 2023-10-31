﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GraphX.Controls.Models;

namespace GraphX.Controls.Animations
{
    public sealed class DeleteShrinkAnimation : IOneWayControlAnimation
    {
        public double Duration { get; set; }
        public bool Centered { get; set; }

        public DeleteShrinkAnimation(double duration = .3, bool centered = true)
        {
            Duration = duration;
            Centered = centered;
        }

        public void AnimateVertex(VertexControl target, bool removeDataVertex = false)
        {
            //get scale transform or create new one
            var transform = CustomHelper.GetScaleTransform(target);
            if (transform == null)
            {
                target.RenderTransform = transform = new ScaleTransform();
                target.RenderTransformOrigin = Centered ? new Point(.5, .5) : new Point(0, 0);
            }
            //create and run animation
            var scaleAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(Duration)));
            Timeline.SetDesiredFrameRate(scaleAnimation, 30);
            scaleAnimation.Completed += (sender, e) => OnCompleted(target, removeDataVertex);
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);            
        }

        public void AnimateEdge(EdgeControl target, bool removeDataEdge = false)
        {
            //ALWAYS fire completed event to init delete procedure after the animation process
            OnCompleted(target, removeDataEdge);
        }

        /// <summary>
        /// Completed event that fires when animation is complete. Must be fired for correct object removal when animation ends.
        /// </summary>
        public event RemoveControlEventHandler? Completed;

        private void OnCompleted(IGraphControl target, bool removeDataObject)
        {
            Completed?.Invoke(this, new ControlEventArgs(target, removeDataObject));
        }
    }
}
