using System.Collections.Generic;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public partial class EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
    {
        protected interface ISegmentContainer : IEnumerable<Segment>, IData, ICloneable
        {
            /// <summary>
            /// Appends the segment <paramref name="s"/> to the end of the 
            /// container.
            /// </summary>
            /// <param name="s">The segment to append.</param>
            void Append(Segment s);

            /// <summary>
            /// Appends all elements of the container <paramref name="sc"/> to 
            /// this container.
            /// </summary>
            /// <param name="sc"></param>
            void Join(ISegmentContainer sc);

            /// <summary>
            /// Split this container at segment <paramref name="s"/> into two contsiners
            /// <paramref name="sc1"/> and <paramref name="sc2"/>. 
            /// All elements less than s are stored in container <paramref name="sc1"/> and
            /// those who are greated than <paramref name="s"/> in <paramref name="sc2"/>.
            /// Element <paramref name="s"/> is neither in <paramref name="sc1"/> or 
            /// <paramref name="sc2"/>.
            /// </summary>
            /// <param name="s">The segment to split at.</param>
            /// <param name="sc1">The container which contains the elements before <paramref name="s"/>.</param>
            /// <param name="sc2">The container which contains the elements after <paramref name="s"/>.</param>
            void Split(Segment s, out ISegmentContainer sc1, out ISegmentContainer sc2);

            /// <summary>
            /// Split the container at position <paramref name="k"/>. The first <paramref name="k"/>
            /// elements of the container are stored in <paramref name="sc1"/> and the remainder
            /// in <paramref name="sc2"/>.
            /// </summary>
            /// <param name="k">The index where the container should be splitted.</param>
            /// <param name="sc1">The container which contains the elements before <paramref name="s"/>.</param>
            /// <param name="sc2">The container which contains the elements after <paramref name="s"/>.</param>
            void Split(int k, out ISegmentContainer sc1, out ISegmentContainer sc2);

            int Count { get; }
        }

        //TODO: implement it with a SplayTree
        //info could be found at:
        //http://en.wikipedia.org/wiki/Splay_tree
        //
        //Implementation that could be ported can be found at:
        //http://www.link.cs.cmu.edu/link/ftp-site/splaying/SplayTree.java
        protected class SegmentContainer : List<Segment>, ISegmentContainer
        {

            public SegmentContainer() { }
            public SegmentContainer(int capacity)
                : base(capacity) { }

            #region ISegmentContainer Members

            public void Append(Segment s)
            {
                Add(s);
            }

            public void Join(ISegmentContainer sc)
            {
                AddRange(sc);
            }

            public void Split(Segment s, out ISegmentContainer sc1, out ISegmentContainer sc2)
            {
                //Contract.Requires(Contains(s));
                //Contract.Ensures(sc1 != null);
                //Contract.Ensures(sc2 != null);

                int index = IndexOf(s);
                Split(index, out sc1, out sc2, false);
            }

            public void Split(int k, out ISegmentContainer sc1, out ISegmentContainer sc2)
            {
                //Contract.Requires(k < Count);
                //Contract.Ensures(sc1 != null);
                //Contract.Ensures(sc2 != null);

                Split(k, out sc1, out sc2, true);
            }

            protected void Split(int k, out ISegmentContainer sc1, out ISegmentContainer sc2, bool keep)
            {
                //Contract.Requires(k < Count);
                //Contract.Ensures(sc1 != null);
                //Contract.Ensures(sc2 != null);

                int sc1Count = k + (keep ? 1 : 0);
                int sc2Count = Count - k - 1;

                sc1 = new SegmentContainer(sc1Count);
                sc2 = new SegmentContainer(sc2Count);

                for (int i = 0; i < sc1Count; i++)
                    sc1.Append(this[i]);

                for (int i = k + 1; i < Count; i++)
                    sc2.Append(this[i]);
            }
            #endregion

            #region IData Members

            //TODO get them from the first element of the container, MAYBE!
            public int Position { get; set; }

            #endregion

            #region ICloneable Members

            public object Clone()
            {
                return this.MemberwiseClone();
            }

            #endregion
        }
    }
}
