using System.Threading;
using System;
using System.Collections.Generic;
using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;

namespace ShowcaseApp.WPF
{
    public class ExampleExternalOverlapRemovalAlgorithm: IExternalOverlapRemoval<DataVertex>
    {
        public IDictionary<DataVertex, Rect> Rectangles { get; set; }

        public void Compute(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
