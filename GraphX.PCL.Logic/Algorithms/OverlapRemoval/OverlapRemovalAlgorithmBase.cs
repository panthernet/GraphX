using System.Collections.Generic;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Logic.Algorithms.OverlapRemoval
{
	public abstract class OverlapRemovalAlgorithmBase<TObject, TParam> : AlgorithmBase, IOverlapRemovalAlgorithm<TObject, TParam>
		where TObject : class
		where TParam : IOverlapRemovalParameters
	{
		protected IDictionary<TObject, Rect> OriginalRectangles;
		public IDictionary<TObject, Rect> Rectangles
		{
			get { return OriginalRectangles; }
            set { OriginalRectangles = value; }
		}

        /// <summary>
        /// Algorithm parameters
        /// </summary>
		public TParam Parameters { get; private set; }

		public IOverlapRemovalParameters GetParameters()
		{
			return Parameters;
		}

		protected List<RectangleWrapper<TObject>> WrappedRectangles;

	    protected OverlapRemovalAlgorithmBase(IDictionary<TObject, Rect> rectangles, TParam parameters )
		{
			OriginalRectangles = rectangles;
			Parameters = parameters;
		}

        /// <summary>
        /// Initialize algorithm initial data
        /// </summary>
        /// <param name="rectangles">Size rectangles</param>
        /// <param name="parameters">algorithm parameters</param>
	    public void Initialize(IDictionary<TObject, Rect> rectangles, TParam parameters)
	    {
            OriginalRectangles = rectangles;
            Parameters = parameters;	        
	    }

        /// <summary>
        /// Initialize algorithm initial data
        /// </summary>
        /// <param name="rectangles">Size rectangles</param>
        public void Initialize(IDictionary<TObject, Rect> rectangles)
        {
            OriginalRectangles = rectangles;
        }

        private void GenerateWrappedRectangles(IDictionary<TObject, Rect> rectangles)
        {
            //wrapping the old rectangles, to remember which one belongs to which object
            WrappedRectangles = new List<RectangleWrapper<TObject>>();
            int i = 0;
            foreach (var kvpRect in rectangles)
            {
                WrappedRectangles.Insert(i, new RectangleWrapper<TObject>(kvpRect.Value, kvpRect.Key));
                i++;
            }
        }

        public sealed override void Compute(CancellationToken cancellationToken)
		{
            GenerateWrappedRectangles(OriginalRectangles);

			AddGaps();

			RemoveOverlap(cancellationToken);

			RemoveGaps();

			foreach ( var r in WrappedRectangles )
				OriginalRectangles[r.Id] = r.Rectangle;
		}

		protected virtual void AddGaps()
		{
			foreach ( var r in WrappedRectangles )
			{
				r.Rectangle.Width += Parameters.HorizontalGap;
				r.Rectangle.Height += Parameters.VerticalGap;
				r.Rectangle.Offset( -Parameters.HorizontalGap / 2, -Parameters.VerticalGap / 2 );
			}
		}

		protected virtual void RemoveGaps()
		{
			foreach ( var r in WrappedRectangles )
			{
				r.Rectangle.Width -= Parameters.HorizontalGap;
				r.Rectangle.Height -= Parameters.VerticalGap;
				r.Rectangle.Offset( Parameters.HorizontalGap / 2, Parameters.VerticalGap / 2 );
			}
		}

		protected abstract void RemoveOverlap(CancellationToken cancellationToken);
	}
}