using System;

namespace GraphX.PCL.Common.Interfaces
{
    public interface IGraphXVertex : IEquatable<IGraphXVertex>, IIdentifiableGraphDataObject
    {
        /// <summary>
        /// Gets or sets custom angle associated with the vertex
        /// </summary>
        double Angle { get; set; }

        /// <summary>
        /// Gets or sets optional group identificator
        /// </summary>
        int GroupId { get; set; }
    }
}
