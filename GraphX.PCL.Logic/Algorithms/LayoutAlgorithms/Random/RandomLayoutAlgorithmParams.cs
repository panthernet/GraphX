using GraphX.Measure;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public class RandomLayoutAlgorithmParams: LayoutParametersBase
    {
        /// <summary>
        /// Gets or sets layout bounds 
        /// </summary>
        public Rect Bounds { get; set; }

        public RandomLayoutAlgorithmParams()
        {
            Bounds = new Rect(0, 0, 2000, 2000);
        }
    }
}
