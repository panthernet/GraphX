using GraphX.GraphSharp;
using GraphX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using YAXLib;

namespace ShowcaseExample
{
    [Serializable]
    public class DataEdge : EdgeBase<DataVertex>
    {
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
			: base(source, target, weight)
		{
            Angle = 90;
		}

        public DataEdge()
            : base(null, null, 1)
        {
            Angle = 90;
        }

        public double Angle { get; set; }

        /// <summary>
        /// Node main description (header)
        /// </summary>
        public string Text { get; set; }
        public string ToolTipText {get; set; }

        public override string ToString()
        {
            return Text;
        }

        [YAXDontSerialize]
        public DataEdge Self
        {
            get { return this; }
        }

    }
}
