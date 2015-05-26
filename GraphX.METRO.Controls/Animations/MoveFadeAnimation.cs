using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using GraphX.PCL.Common.Exceptions;

namespace GraphX.Controls.Animations
{
    public sealed class MoveFadeAnimation : MoveAnimationBase
    {
        public MoveFadeAnimation(TimeSpan duration)
        {
            Duration = duration;
        }
        int _vMaxCount;
        int _vCounter;


        private bool isDefaultCoordinates()
        {
            var ptZero = new Point();
            return VertexStorage.Keys.All(item => item.GetPosition() == ptZero);
        }

        public override void RunVertexAnimation()
        {
            //custom event signal preparations
            _vMaxCount = VertexStorage.Count;
            _vCounter = 0;
            var defaultValues = isDefaultCoordinates();

            foreach (var item in VertexStorage)
            {
                if (item.Key is EdgeControl) throw new GX_InvalidDataException("AnimateVertex() -> Got edge control instead vertex control!");

                var control = item.Key as Control;
                //GraphAreaBase.SetFinalX(control, item.Value.X);
                //GraphAreaBase.SetFinalY(control, item.Value.Y);
                //double.IsNaN(GraphAreaBase.GetX(control))
                if (defaultValues)
                {
                    GraphAreaBase.SetX(control, GraphAreaBase.GetFinalX(control));
                    GraphAreaBase.SetY(control, GraphAreaBase.GetFinalY(control));
                    //control.Opacity = 0;
                    CreateStory(control, 0, 1, (o2, e2) =>
                    {
                        _vCounter++;
                        if (_vCounter == _vMaxCount)
                            OnCompleted();
                    }).Begin();
                } else
                {
                    CreateStory(control, 1, 0, (o, e) =>
                    {
                        if (!VertexStorage.ContainsKey(item.Key))
                            return; //just in case of... who knows what?
                        GraphAreaBase.SetX(control, GraphAreaBase.GetFinalX(control));
                        GraphAreaBase.SetY(control, GraphAreaBase.GetFinalY(control));

                        CreateStory(control, 0, 1, (o2, e2) =>
                        {
                            _vCounter++;
                            if (_vCounter == _vMaxCount)
                                OnCompleted();
                        }).Begin();

                    }).Begin();
                }
            }
        }

        public override void RunEdgeAnimation()
        {
            foreach(var item in EdgeStorage)
            {
                if (item is VertexControl) throw new GX_InvalidDataException("AnimateEdge() -> Got vertex control instead edge control!");
                var control = item as Control;
                CreateStory(control, 1, 0, (o, e) => CreateStory(control, 0, 1).Begin()).Begin();
            }
        }

        /// <summary>
        /// Storyboard creation
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="start">Start param value</param>
        /// <param name="end">End Param value</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private Storyboard CreateStory(Control control, double start, double end, EventHandler<object> callback = null)
        {
            var story = new Storyboard();
            var fadeAnimation = new DoubleAnimation()
            {
                From = start,
                To = end,
                Duration = new Duration(Duration)
            };
            if (callback != null) story.Completed += callback;
            story.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, control);
            Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
            return story;
        }


        public override void Cleanup()
        {
            
        }
    }
}