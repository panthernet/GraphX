using GraphX.Controls.Models;

namespace GraphX.Controls.Animations
{
    public interface IOneWayControlAnimation
    {
        /// <summary>
        /// Animation duration
        /// </summary>
        double Duration { get; set; }
        /// <summary>
        /// Run vertex animation
        /// </summary>
        /// <param name="target"></param>
        void AnimateVertex(VertexControl target);
        /// <summary>
        /// Run edge animation
        /// </summary>
        /// <param name="target"></param>
        void AnimateEdge(EdgeControl target);
        /// <summary>
        /// Completed event that fires when animation is complete. Must be fired for correct object removal when animation ends.
        /// </summary>
        event RemoveControlEventHandler Completed;
    }
}
