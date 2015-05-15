using GraphX.Measure;

namespace GraphX.PCL.Logic.Algorithms.OverlapRemoval
{
	/// <summary>
	/// A System.Windows.Rect egy struktúra, ezért a heap-en tárolódik. Bizonyos esetekben ez nem
	/// szerencsés, így szükség van erre a wrapper osztályra. Mivel ez class, ezért nem
	/// érték szerinti átadás van.
	/// </summary>
	public class RectangleWrapper<TObject>
		where TObject : class
	{
		private readonly TObject id;
		public TObject Id
		{
			get { return id; }
		}

		public Rect Rectangle;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rectangle"></param>
		/// <param name="id">Az adott téglalap azonosítója (az overlap-removal végén tudnunk kell, hogy 
		/// melyik téglalap melyik objektumhoz tartozik. Az azonosítás megoldható lesz id alapján.</param>
		public RectangleWrapper( Rect rectangle, TObject id )
		{
			Rectangle = rectangle;
			this.id = id;
		}

		public double CenterX
		{
			get { return Rectangle.Left + Rectangle.Width / 2; }
		}

		public double CenterY
		{
			get { return Rectangle.Top + Rectangle.Height / 2; }
		}

		public Point Center
		{
			get { return new Point( CenterX, CenterY ); }
		}
	}
}