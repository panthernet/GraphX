using System.Threading;

namespace GraphX.GraphSharp.Algorithms
{
	public abstract class AlgorithmBase : IAlgorithm
	{
	    public abstract void Compute(CancellationToken cancellationToken);
	}
}