using System;
using GraphX.Measure;
using YAXLib;
using YAXLib.Customization;

namespace ShowcaseApp.WPF.FileSerialization
{
    public sealed class YAXPointSerializer: ICustomSerializer<Point>
    {

        private Point Deserialize(string str)
        {
            var res = str.Split(new char[] { '|' });
            if (res.Length == 2) return new Point(Convert.ToDouble(res[0]), Convert.ToDouble(res[1]));
            else return new Point();
        }

        public Point DeserializeFromAttribute(System.Xml.Linq.XAttribute attrib, ISerializationContext serializationContext)
        {
            return Deserialize(attrib.Value);
        }

        public Point DeserializeFromElement(System.Xml.Linq.XElement element, ISerializationContext serializationContext)
        {
            return Deserialize(element.Value);
        }

        public Point DeserializeFromValue(string value, ISerializationContext serializationContext)
        {
            return Deserialize(value);
        }

        public void SerializeToAttribute(Point objectToSerialize, System.Xml.Linq.XAttribute attrToFill, ISerializationContext serializationContext)
        {
            attrToFill.Value = String.Format("{0}|{1}", objectToSerialize.X.ToString(), objectToSerialize.Y.ToString());
        }

        public void SerializeToElement(Point objectToSerialize, System.Xml.Linq.XElement elemToFill, ISerializationContext serializationContext)
        {
            elemToFill.Value = String.Format("{0}|{1}", objectToSerialize.X.ToString(), objectToSerialize.Y.ToString());
        }

        public string SerializeToValue(Point objectToSerialize, ISerializationContext serializationContext)
        {
            return String.Format("{0}|{1}", objectToSerialize.X.ToString(), objectToSerialize.Y.ToString());
        }
    }
}
