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
        /// <param name="removeDataVertex">Remove data vertex from data graph when animation is finished. Default value is False.</param>
        void AnimateVertex(VertexControl target, bool removeDataVertex = false);
        /// <summary>
        /// Run edge animation
        /// </summary>
        /// <param name="target"></param>
        /// <param name="removeDataEdge">Remove data edge from data graph when animation is finished. Default value is False.</param>
        void AnimateEdge(EdgeControl target, bool removeDataEdge = false);
        /// <summary>
        /// Completed event that fires when animation is complete. Must be fired for correct object removal when animation ends.
        /// </summary>
        event RemoveControlEventHandler Completed;
    }
}
