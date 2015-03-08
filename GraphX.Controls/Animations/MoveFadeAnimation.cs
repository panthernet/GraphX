using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using GraphX.Controls.Models.Interfaces;

namespace GraphX.Animations
{
    public sealed class MoveFadeAnimation : MoveAnimationBase
    {
        public MoveFadeAnimation(TimeSpan duration)
        {
            Duration = duration;
        }
        int _vMaxCount;
        int _eMaxCount;
        int _vCounter;
        int _eCounter;
        public override void RunVertexAnimation()
        {
            //custom event signal preparations
            _vMaxCount = VertexStorage.Count;
            _eMaxCount = EdgeStorage.Count;
            _vCounter = 0; _eCounter = 0;
            foreach (var item in VertexStorage)
            {
                if (item.Key is EdgeControl) throw new GX_InvalidDataException("AnimateVertex() -> Got edge control instead vertex control!");
                var story = CreateStory(item.Key, 1, 0, VertexAnimation_Completed);
                story.Completed += VertexAnimation_Completed;
                story.Begin(item.Key as Control, HandoffBehavior.Compose);
                
            }
        }

        void VertexAnimation_Completed(object sender, EventArgs e)
        {
            if (sender is ClockGroup) return;
            var control = (IGraphControl)Storyboard.GetTarget((sender as AnimationClock).Timeline);
            if (!VertexStorage.ContainsKey(control))
            {
                return; //just in case of... who knows what?
            }
            control.SetPosition(VertexStorage[control].X, VertexStorage[control].Y, false);

            VertexStorage.Remove(control);
            var story = CreateStory(control, 0, 1, null);
            story.Completed += story_Completed;
            story.Begin(control as Control, HandoffBehavior.Compose);
        }

        void story_Completed(object sender, EventArgs e)
        {
            //count successful completeions and fire event when all animations are complete
            _vCounter++;
            _eCounter++;
            if(_vCounter == _vMaxCount && _eCounter == _eMaxCount) OnCompleted();
        }

        public override void RunEdgeAnimation()
        {
            foreach(var item in EdgeStorage)
            {
                if (item is VertexControl) throw new GX_InvalidDataException("AnimateEdge() -> Got vertex control instead edge control!");
                var story = CreateStory(item, 1, 0, EdgeAnimation_Completed);
                story.Completed += EdgeAnimation_Completed;
                story.Begin(item as Control, HandoffBehavior.Compose);
            }
        }

        void EdgeAnimation_Completed(object sender, EventArgs e)
        {
            if (sender is ClockGroup) return;
            var control = (IGraphControl)Storyboard.GetTarget((sender as AnimationClock).Timeline);
            var story = CreateStory(control, 0, 1, null);
            EdgeStorage.Remove(control);
            story.Completed+=story_Completed;
            story.Begin(control as Control, HandoffBehavior.Compose);
        }

        /// <summary>
        /// Storyboard creation
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="start">Start param value</param>
        /// <param name="end">End Param value</param>
        /// <returns></returns>
        private Storyboard CreateStory(IGraphControl control, double start, double end, EventHandler callback)
        {
            var story = new Storyboard();
            var fadeAnimation = new DoubleAnimation(start, end, new Duration(Duration), FillBehavior.Stop);
            if(callback != null) fadeAnimation.Completed += callback;
            story.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, control as Control);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            return story;
        }


        public override void Cleanup()
        {
            
        }
    }
}