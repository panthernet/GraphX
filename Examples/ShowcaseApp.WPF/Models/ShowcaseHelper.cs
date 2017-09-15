using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace ShowcaseApp.WPF.Models
{
    public static class ShowcaseHelper
    {
        static ShowcaseHelper()
        {
            GenerateData(DATASOURCE_SIZE);
        }
    
        /// <summary>
        /// General data source
        /// </summary>
        public static List<DataItem> DataSource { get; set; }

        public const int DATASOURCE_SIZE = 5000;
        public static readonly Random Rand = new Random(Guid.NewGuid().GetHashCode());

        private static void GenerateData(int count)
        {
            var list = new List<DataItem>();
            for (var i = 0; i < count; i++)
            {
                list.Add(new DataItem { ID = i, Text = "Vertex " + i });
            }
            DataSource = list;
        }

        public static void AddEdge(BidirectionalGraph<DataVertex, DataEdge> graph, DataVertex source, DataVertex target, int? sourcePoint = null, int? targetPoint = null, int weight = 0)
        {
            var edge = new DataEdge(source, target, weight)
            {
                ID = Rand.Next(),
                Text = string.Empty,
                SourceConnectionPointId = sourcePoint,
                TargetConnectionPointId = targetPoint,
                ToolTipText = "Label "+ source.ID
            };

            graph.AddEdge(edge);
        }

        public static DataEdge GenerateEdge(DataVertex source, DataVertex target, int weight = 0)
        {
            return new DataEdge(source, target, weight) 
            {
                Text = string.Empty
            };
        }

        #region GenerateDataGraph

        /// <summary>
        /// Generate example graph data
        /// </summary>
        /// <param name="count">Items count</param>
        /// <param name="addEdges"></param>
        public static GraphExample GenerateDataGraph(int count, bool addEdges = true, int edgeCountMult = 25)
        {
            var graph = new GraphExample();

            foreach (var item in DataSource.Take(count))
                graph.AddVertex(new DataVertex(item.Text) { ID = item.ID, ImageId = Rand.Next(0,3)});

            var vlist = graph.Vertices.ToList();
            var edgeId = count + 1;

            if (!addEdges) return graph;

            foreach (var item in vlist)
            {
                if (Rand.Next(0, 50) > edgeCountMult) continue;
                var vertex2 = vlist[Rand.Next(0, graph.VertexCount - 1)];
                var txt = string.Format("{0} -> {1}", item.Text, vertex2.Text);
                graph.AddEdge(new DataEdge(item, vertex2, Rand.Next(1, 50)) { ID = edgeId, Text = txt, ToolTipText = txt });
                edgeId++;
            }
            return graph;
        }
        #endregion

        public static GraphExample GenerateSugiDataGraph()
        {
            var graph = new GraphExample();
            foreach (var item in DataSource.Take(25))
                graph.AddVertex(new DataVertex(item.Text) { ID = item.ID, ImageId = Rand.Next(0, 3) });

            var vList = graph.Vertices.ToList();
            var edgeId = 1;
            //1 tier
            graph.AddNewEdge(vList[0], vList[1], ++edgeId);
            graph.AddNewEdge(vList[0], vList[2], ++edgeId);
            graph.AddNewEdge(vList[0], vList[3], ++edgeId);
            graph.AddNewEdge(vList[0], vList[4], ++edgeId);
            //2 tier
            graph.AddNewEdge(vList[1], vList[5], ++edgeId);
            graph.AddNewEdge(vList[1], vList[6], ++edgeId);
            graph.AddNewEdge(vList[2], vList[7], ++edgeId);
            graph.AddNewEdge(vList[2], vList[8], ++edgeId);
            graph.AddNewEdge(vList[3], vList[1], ++edgeId);
            graph.AddNewEdge(vList[4], vList[9], ++edgeId);
            graph.AddNewEdge(vList[4], vList[9], ++edgeId);
            //3 tier
            graph.AddNewEdge(vList[8], vList[10], ++edgeId);
            graph.AddNewEdge(vList[8], vList[11], ++edgeId);
            graph.AddNewEdge(vList[8], vList[12], ++edgeId);

            return graph;
        }

        private static void AddNewEdge(this GraphExample graph, DataVertex source, DataVertex target, long id)
        {
            var text = string.Format("{0} -> {1}", source.Text, target.Text);
            graph.AddEdge(new DataEdge(source, target, Rand.Next(1, 50)) { ID = id, Text = text, ToolTipText = text });
            
        }
    }
}
