using System.Collections.Generic;
using System.Threading.Tasks;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Common.Models;

namespace GraphX.METRO.Controls.Models
{
    /// <summary>
    /// WPF implementation of IFileServiceProvider
    /// </summary>
    public class FileServiceProviderMETRO: IFileServiceProvider
    {
        /// <summary>
        /// Serializes data classes list to file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="modelsList">Data classes list</param>
        public async void SerializeDataToFile(string filename, List<GraphSerializationData> modelsList)
        {
            //TODO serializer
           /* var serializer = new YAXSerializer(typeof(List<GraphSerializationData>));
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filename);
            var or = await file.OpenAsync(FileAccessMode.ReadWrite);
            using (var textWriter = new StreamWriter(or.AsStreamForWrite()))
            {
                serializer.Serialize(modelsList, textWriter);
                textWriter.Close();
            }*/
        }
        /// <summary>
        /// Deserializes data classes list from file
        /// </summary>
        /// <param name="filename">File name</param>
        public List<GraphSerializationData> DeserializeDataFromFile(string filename)
        {

            return Deserialize(filename).Result;
        }

        private async Task<List<GraphSerializationData>> Deserialize(string filename)
        {
            //TODO deserializer
            /*var deserializer = new YAXSerializer(typeof(List<GraphSerializationData>));
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filename);
            var or = await file.OpenReadAsync();
            using (var textReader = new StreamReader(or.AsStreamForRead()))
            {
                return (List<GraphSerializationData>)deserializer.Deserialize(textReader);
            }*/
            return new List<GraphSerializationData>();
        }
    }
}
