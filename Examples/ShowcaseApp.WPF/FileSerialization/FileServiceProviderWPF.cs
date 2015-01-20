using System.Collections.Generic;
using System.IO;
using GraphX;
using GraphX.Models;
using YAXLib;

namespace ShowcaseApp.WPF.FileSerialization
{
    /// <summary>
    /// WPF implementation of IFileServiceProvider
    /// </summary>
    public class FileServiceProviderWpf: IFileServiceProvider
    {
        /// <summary>
        /// Serializes data classes list to file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="modelsList">Data classes list</param>
        public void SerializeDataToFile(string filename, List<GraphSerializationData> modelsList)
        {
            var serializer = new YAXSerializer(typeof(List<GraphSerializationData>));
            using (var textWriter = new StreamWriter(filename))
            {
                serializer.Serialize(modelsList, textWriter);
                textWriter.Close();
            }
        }
        /// <summary>
        /// Deserializes data classes list from file
        /// </summary>
        /// <param name="filename">File name</param>
        public List<GraphSerializationData> DeserializeDataFromFile(string filename)
        {
            var deserializer = new YAXSerializer(typeof(List<GraphSerializationData>));
            using (var textReader = new StreamReader(filename))
            {
                return (List<GraphSerializationData>)deserializer.Deserialize(textReader);
            }
        }
    }
}
