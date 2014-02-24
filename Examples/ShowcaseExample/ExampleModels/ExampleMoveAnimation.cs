using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphX.Animations;

namespace ShowcaseExample.ExampleModels
{
    public class ExampleMoveAnimation: MoveAnimationBase
    {
        public ExampleMoveAnimation()
        {
            //set animation duration
            Duration = TimeSpan.FromSeconds(1);
        }

        public override void Cleanup()
        {
            //cleanup something
        }

        public override void RunVertexAnimation()
        {
            //do some vertex animation. Can be optional.

            //you need to call this manually at the animation end
            //if you want corresponding event fired on complete
            OnCompleted();
        }

        public override void RunEdgeAnimation()
        {
            //do some edge animation. Can be optional.
        }
    }
}
