using System;
using GraphX.Measure;
using YAXLib;

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

        public Point DeserializeFromAttribute(System.Xml.Linq.XAttribute attrib)
        {
            return Deserialize(attrib.Value);
        }

        public Point DeserializeFromElement(System.Xml.Linq.XElement element)
        {
            return Deserialize(element.Value);
        }

        public Point DeserializeFromValue(string value)
        {
            return Deserialize(value);
        }

        public void SerializeToAttribute(Point objectToSerialize, System.Xml.Linq.XAttribute attrToFill)
        {
            attrToFill.Value = String.Format("{0}|{1}", objectToSerialize.X.ToString(), objectToSerialize.Y.ToString());
        }

        public void SerializeToElement(Point objectToSerialize, System.Xml.Linq.XElement elemToFill)
        {
            elemToFill.Value = String.Format("{0}|{1}", objectToSerialize.X.ToString(), objectToSerialize.Y.ToString());
        }

        public string SerializeToValue(Point objectToSerialize)
        {
            return String.Format("{0}|{1}", objectToSerialize.X.ToString(), objectToSerialize.Y.ToString());
        }
    }
}
