using System;
using QuickGraph.Algorithms;

namespace GraphX.GraphSharp.Algorithms
{
	/// <summary>
	/// Simple algorithm interface which is not connected to any graph.
	/// </summary>
	public interface IAlgorithm
	{
		object SyncRoot { get;}
		ComputationState State { get;}

		void Compute();
		void Abort();

		event EventHandler StateChanged;
		event EventHandler Started;
		event EventHandler Finished;
		event EventHandler Aborted;
	}
}