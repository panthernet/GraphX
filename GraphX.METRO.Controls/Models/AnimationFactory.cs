using System;
using GraphX.Controls.Animations;

namespace GraphX.Controls.Models
{
    public static class AnimationFactory
    {
        /// <summary>
        /// Create move animation by supplied type
        /// </summary>
        /// <param name="type">Animation type</param>
        /// <param name="duration">Animation duration</param>
        public static MoveAnimationBase CreateMoveAnimation(MoveAnimation type, TimeSpan duration)
        {
            switch (type)
            {
                case MoveAnimation.None: 
                    return null;
                case MoveAnimation.Move:
                    return new MoveSimpleAnimation(duration);
                case MoveAnimation.Fade:
                    return new MoveFadeAnimation(duration);
                
            }
            return null;
        }

        public static IOneWayControlAnimation CreateDeleteAnimation(DeleteAnimation type, double duration = .3)
        {
            switch (type)
            {
                case DeleteAnimation.None:
                    return null;
                case DeleteAnimation.Shrink:
                    return new DeleteShrinkAnimation(duration);
                case DeleteAnimation.Fade:
                    return new DeleteFadeAnimation(duration);

            }
            return null;
        }

        public static IBidirectionalControlAnimation CreateMouseOverAnimation(MouseOverAnimation type, double duration = .3)
        {
            switch (type)
            {
                case MouseOverAnimation.None:
                    return null;
                case MouseOverAnimation.Scale:
                    return new MouseOverScaleAnimation(duration);
            }
            return null;
        }
    }
}
