using System.Threading;

namespace GraphX.PCL.Logic.Algorithms
{
	public abstract class AlgorithmBase : IAlgorithm
	{
	    public abstract void Compute(CancellationToken cancellationToken);
	}
}