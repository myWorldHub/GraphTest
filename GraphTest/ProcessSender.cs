using System.Threading.Tasks;
using GraphConnectEngine.Core;
using GraphConnectEngine.Graph.Event;
using GraphConnectEngine.Node;

namespace GraphTest
{
    public class ProcessSender : IProcessSender
    {
        public async Task Fire(OutProcessNode node)
        {
            await node.CallProcess(ProcessCallArgs.Fire(node));
        }
    }
}