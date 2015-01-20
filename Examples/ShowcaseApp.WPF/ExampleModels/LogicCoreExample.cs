using GraphX.Logic;
using QuickGraph;
using ShowcaseApp.WPF.FileSerialization;

namespace ShowcaseApp.WPF
{
    public class LogicCoreExample : GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
    {
        public LogicCoreExample()
        {
            FileServiceProvider = new FileServiceProviderWpf();
        }
    }
}
