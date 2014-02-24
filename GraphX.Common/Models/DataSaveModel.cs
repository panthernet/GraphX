using GraphX.Models.XmlSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using YAXLib;

namespace GraphX.Models
{
    public class DataSaveModel
    {
        public object Data { get; set; }
        [YAXCustomSerializer(typeof(YAXPointSerializer))]
        public Point Position { get; set; }
    }
}
