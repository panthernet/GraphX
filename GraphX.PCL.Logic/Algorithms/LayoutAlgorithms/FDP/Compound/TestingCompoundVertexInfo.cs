using GraphX.Measure;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public class TestingCompoundVertexInfo
    {
        public TestingCompoundVertexInfo(Vector springForce, Vector repulsionForce, Vector gravityForce, Vector applicationForce)
        {
            SpringForce = springForce;
            RepulsionForce = repulsionForce;
            GravityForce = gravityForce;
            ApplicationForce = applicationForce;
        }

        public Vector SpringForce { get; set; }
        public Vector RepulsionForce { get; set; }
        public Vector GravityForce { get; set; }
        public Vector ApplicationForce { get; set; }
    }
}
