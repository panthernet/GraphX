using System.Collections.Generic;
using GraphX.Measure;

namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
	public interface IOverlapRemovalContext<TVertex>
	{
		IDictionary<TVertex, Rect> Rectangles { get; }
	}
}