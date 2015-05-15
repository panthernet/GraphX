using System.Collections.Generic;
using GraphX.PCL.Common.Models;

namespace GraphX.PCL.Common.Interfaces
{
    public interface IFileServiceProvider
    {
        /// <summary>
        /// Serializes specified list of data classes into file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="modelsList">Data classes list</param>
        void SerializeDataToFile(string filename, List<GraphSerializationData> modelsList);
        /// <summary>
        /// Deserializes specified file data into data classes list
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        List<GraphSerializationData> DeserializeDataFromFile(string filename);
    }
}
