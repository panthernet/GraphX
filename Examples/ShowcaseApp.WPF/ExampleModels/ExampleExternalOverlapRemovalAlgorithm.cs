using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using System;
using System.Collections.Generic;
using GraphX.Measure;

namespace ShowcaseApp.WPF
{
    public class ExampleExternalOverlapRemovalAlgorithm: IExternalOverlapRemoval<DataVertex>
    {
        public IDictionary<DataVertex, Rect> Rectangles { get; set; }

        public void Compute()
        {
            throw new NotImplementedException();
        }
    }
}
