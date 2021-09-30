using System.Threading;
using System.Threading.Tasks;
using GraphConnectEngine.Core;
using GraphConnectEngine.Graph.Event;
using GraphConnectEngine.Node;

namespace GraphTest
{
    public class ProcessSender : IProcessSender
    {

        private SemaphoreSlim sem = new SemaphoreSlim(1);
        public async Task Fire(OutProcessNode node)
        {
            await sem.WaitAsync();
            await node.CallProcess(ProcessCallArgs.Fire(node));
            sem.Release();
        }
    }
}