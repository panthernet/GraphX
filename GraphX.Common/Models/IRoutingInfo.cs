using System.Windows;

namespace GraphX
{
    public interface IRoutingInfo
    {
        /// <summary>
        /// Routing points collection used to make Path visual object
        /// </summary>
        Point[] RoutingPoints { get; set; }
    }
}
