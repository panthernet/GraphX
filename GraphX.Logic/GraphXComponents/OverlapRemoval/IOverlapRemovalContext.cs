using System.Collections.Generic;
using System.Windows;

namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
	public interface IOverlapRemovalContext<TVertex>
	{
		IDictionary<TVertex, Rect> Rectangles { get; }
	}
}