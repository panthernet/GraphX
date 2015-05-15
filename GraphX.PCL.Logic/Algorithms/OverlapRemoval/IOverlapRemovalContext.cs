using System.Collections.Generic;
using GraphX.Measure;

namespace GraphX.PCL.Logic.Algorithms.OverlapRemoval
{
	public interface IOverlapRemovalContext<TVertex>
	{
		IDictionary<TVertex, Rect> Rectangles { get; }
	}
}