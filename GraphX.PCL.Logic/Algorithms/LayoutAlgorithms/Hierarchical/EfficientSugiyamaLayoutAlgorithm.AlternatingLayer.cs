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
        protected class AlternatingLayer : List<IData>, ICloneable
        {
            /// <summary>
            /// This method ensures that the layer is a real alternating
            /// layer: starts with a SegmentContainer followed by a Vertex,
            /// another SegmentContainer, another Vertex, ... ending wiht 
            /// a SegmentContainer.
            /// </summary>
            public void EnsureAlternatingAndPositions()
            {
                bool shouldBeAContainer = true;
                for (int i = 0; i < Count; i++, shouldBeAContainer = !shouldBeAContainer)
                {
                    if (shouldBeAContainer && this[i] is SugiVertex)
                    {
                        Insert(i, new SegmentContainer());
                    }
                    else
                    {
                        while (i < Count && !shouldBeAContainer && this[i] is SegmentContainer)
                        {
                            //the previous one must be a container too
                            var prevContainer = this[i - 1] as SegmentContainer;
                            var actualContainer = this[i] as SegmentContainer;
                            prevContainer.Join(actualContainer);
                            RemoveAt(i);
                        }
                        if (i >= Count)
                            break;
                    }
                }

                if (shouldBeAContainer)
                {
                    //the last element in the alternating layer 
                    //should be a container, but it's not
                    //so add an empty one
                    Add(new SegmentContainer());
                }
            }

            protected void EnsurePositions()
            {
                //assign positions to vertices on the actualLayer (L_i)
                for (int i = 1; i < this.Count; i += 2)
                {
                    var precedingContainer = this[i - 1] as SegmentContainer;
                    var vertex = this[i] as SugiVertex;
                    if (i == 1)
                    {
                        vertex.Position = precedingContainer.Count;
                    }
                    else
                    {
                        var previousVertex = this[i - 2] as SugiVertex;
                        vertex.Position = previousVertex.Position + precedingContainer.Count + 1;
                    }
                }

                //assign positions to containers on the actualLayer (L_i+1)
                for (int i = 0; i < this.Count; i += 2)
                {
                    var container = this[i] as SegmentContainer;
                    if (i == 0)
                    {
                        container.Position = 0;
                    }
                    else
                    {
                        var precedingVertex = this[i - 1] as SugiVertex;
                        container.Position = precedingVertex.Position + 1;
                    }
                }
            }

            public void SetPositions()
            {
                int nextPosition = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    var segmentContainer = this[i] as SegmentContainer;
                    var vertex = this[i] as SugiVertex;
                    if (segmentContainer != null)
                    {
                        segmentContainer.Position = nextPosition;
                        nextPosition += segmentContainer.Count;
                    }
                    else if (vertex != null)
                    {
                        vertex.Position = nextPosition;
                        nextPosition += 1;
                    }
                }
            }

            public AlternatingLayer Clone()
            {
                var clonedLayer = new AlternatingLayer();
                foreach (var item in this)
                {
                    var cloneableItem = item as ICloneable;
                    if (cloneableItem != null)
                        clonedLayer.Add(cloneableItem.Clone() as IData);
                    else
                        clonedLayer.Add(item);
                }
                return clonedLayer;
            }

            #region ICloneable Members

            object ICloneable.Clone()
            {
                return this.Clone();
            }

            #endregion
        }
    }
}
