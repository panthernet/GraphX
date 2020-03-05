using System.Threading;

namespace GraphX.Logic.Algorithms
{
	public abstract class AlgorithmBase : IAlgorithm
	{
	    public abstract void Compute(CancellationToken cancellationToken);
	}
}