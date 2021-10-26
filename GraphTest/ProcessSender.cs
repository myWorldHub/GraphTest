using System;
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

        private int a = 0;
        public async Task Fire(OutProcessNode node)
        {
            string b = a++ + " : " + UnitTest1.GetUnixTime();
            
            Logger.Debug(b + " : 呼ばれた");
            await sem.WaitAsync();
            Logger.Debug(b + " : 実行開始");
            await node.CallProcess(ProcessCallArgs.Fire(node));
            Logger.Debug(b + " : 完了");
            sem.Release();
        }
    }
}