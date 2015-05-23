using System.Collections.Generic;
using System.IO;
using GraphX.PCL.Common.Models;
using YAXLib;

namespace ShowcaseApp.WPF.FileSerialization
{
    /// <summary>
    /// WPF implementation of file serialization and deserialization
    /// </summary>
    public static class FileServiceProviderWpf
    {
        /// <summary>
        /// Serializes data classes list to file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="modelsList">Data classes list</param>
        public static void SerializeDataToFile(string filename, List<GraphSerializationData> modelsList)
        {
            using (FileStream stream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                SerializeDataToStream(stream, modelsList);
            }
        }

        /// <summary>
        /// Deserializes data classes list from file
        /// </summary>
        /// <param name="filename">File name</param>
		public static List<GraphSerializationData> DeserializeDataFromFile(string filename)
        {
            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return DeserializeDataFromStream(stream);
            }
        }

        /// <summary>
        /// Serializes graph data list to a stream
        /// </summary>
        /// <param name="stream">The destination stream</param>
        /// <param name="modelsList">The graph data</param>
		public static void SerializeDataToStream(Stream stream, List<GraphSerializationData> modelsList)
        {
            var serializer = new YAXSerializer(typeof(List<GraphSerializationData>));
            using (var textWriter = new StreamWriter(stream))
            {
                serializer.Serialize(modelsList, textWriter);
                textWriter.Flush();
            }
        }

        /// <summary>
        /// Deserializes graph data from a stream
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <returns>The graph data</returns>
		public static List<GraphSerializationData> DeserializeDataFromStream(Stream stream)
        {
            var deserializer = new YAXSerializer(typeof(List<GraphSerializationData>));
            using (var textReader = new StreamReader(stream))
            {
                return (List<GraphSerializationData>)deserializer.Deserialize(textReader);
            }
        }
    }
}
