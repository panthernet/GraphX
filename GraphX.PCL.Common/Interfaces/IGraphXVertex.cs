using System;

namespace GraphX
{
    public interface IGraphXVertex : IEquatable<IGraphXVertex>, IIdentifiableGraphDataObject
    {
        /// <summary>
        /// Skip vertex in algorithm calc and visual control generation
        /// </summary>
        bool SkipProcessing { get; }
    }
}
