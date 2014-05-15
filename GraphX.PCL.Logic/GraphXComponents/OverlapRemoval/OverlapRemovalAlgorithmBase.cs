using System.Collections.Generic;
using GraphX.Measure;

namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
	public abstract class OverlapRemovalAlgorithmBase<TObject, TParam> : AlgorithmBase, IOverlapRemovalAlgorithm<TObject, TParam>
		where TObject : class
		where TParam : IOverlapRemovalParameters
	{
		protected IDictionary<TObject, Rect> originalRectangles;
		public IDictionary<TObject, Rect> Rectangles
		{
			get { return originalRectangles; }
            set { originalRectangles = value; }
		}

		public TParam Parameters { get; private set; }

		public IOverlapRemovalParameters GetParameters()
		{
			return Parameters;
		}

		protected List<RectangleWrapper<TObject>> wrappedRectangles;


		public OverlapRemovalAlgorithmBase( IDictionary<TObject, Rect> rectangles, TParam parameters )
		{
			//eredeti téglalapok listája
			originalRectangles = rectangles;

			Parameters = parameters;
		}

        private void GenerateWrappedRectangles(IDictionary<TObject, Rect> rectangles)
        {
            //wrapping the old rectangles, to remember which one belongs to which object
            wrappedRectangles = new List<RectangleWrapper<TObject>>();
            int i = 0;
            foreach (var kvpRect in rectangles)
            {
                wrappedRectangles.Insert(i, new RectangleWrapper<TObject>(kvpRect.Value, kvpRect.Key));
                i++;
            }
        }

		protected sealed override void InternalCompute()
		{
            GenerateWrappedRectangles(originalRectangles);

			AddGaps();

			RemoveOverlap();

			RemoveGaps();

			foreach ( var r in wrappedRectangles )
				originalRectangles[r.Id] = r.Rectangle;
		}

		protected virtual void AddGaps()
		{
			foreach ( var r in wrappedRectangles )
			{
				r.Rectangle.Width += Parameters.HorizontalGap;
				r.Rectangle.Height += Parameters.VerticalGap;
				r.Rectangle.Offset( -Parameters.HorizontalGap / 2, -Parameters.VerticalGap / 2 );
			}
		}

		protected virtual void RemoveGaps()
		{
			foreach ( var r in wrappedRectangles )
			{
				r.Rectangle.Width -= Parameters.HorizontalGap;
				r.Rectangle.Height -= Parameters.VerticalGap;
				r.Rectangle.Offset( Parameters.HorizontalGap / 2, Parameters.VerticalGap / 2 );
			}
		}

		protected abstract void RemoveOverlap();
	}
}