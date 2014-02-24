using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShowcaseExample.ExampleModels
{
    public class ExampleExternalOverlapRemovalAlgorithm: IExternalOverlapRemoval<DataVertex>
    {
        public IDictionary<DataVertex, System.Windows.Rect> Rectangles { get; set; }

        public void Compute()
        {
            throw new NotImplementedException();
        }
    }
}
