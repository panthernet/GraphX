using System.Collections.Generic;
using System.Threading.Tasks;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Common.Models;

namespace GraphX.METRO.Controls.Models
{
    /// <summary>
    /// WPF implementation of IFileServiceProvider
    /// </summary>
    public static class FileServiceProviderMETRO
    {
        /// <summary>
        /// Serializes data classes list to file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="modelsList">Data classes list</param>
        public static async void SerializeDataToFile(string filename, List<GraphSerializationData> modelsList)
        {

        }
        /// <summary>
        /// Deserializes data classes list from file
        /// </summary>
        /// <param name="filename">File name</param>
        public static List<GraphSerializationData> DeserializeDataFromFile(string filename)
        {

            return Deserialize(filename).Result;
        }

        private static async Task<List<GraphSerializationData>> Deserialize(string filename)
        {
            return new List<GraphSerializationData>();
        }
    }
}
