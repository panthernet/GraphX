using System.Threading;

namespace GraphX.GraphSharp.Algorithms
{
	/// <summary>
	/// Simple algorithm interface which is not connected to any graph.
	/// </summary>
	public interface IAlgorithm
	{
        void Compute(CancellationToken cancellationToken);
	}
}