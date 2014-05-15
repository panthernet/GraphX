using System.Collections.Generic;

namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
	public interface IOverlapRemovalAlgorithmFactory<TVertex>
		where TVertex : class
	{
		/// <summary>
		/// List of the available algorithms.
		/// </summary>
		IEnumerable<string> AlgorithmTypes { get; }

		IOverlapRemovalAlgorithm<TVertex> CreateAlgorithm( string newAlgorithmType, IOverlapRemovalContext<TVertex> context, IOverlapRemovalParameters parameters );

		IOverlapRemovalParameters CreateParameters( string algorithmType, IOverlapRemovalParameters oldParameters );

		bool IsValidAlgorithm( string algorithmType );

		string GetAlgorithmType( IOverlapRemovalAlgorithm<TVertex> algorithm );
	}
}