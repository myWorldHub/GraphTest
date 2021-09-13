using System;
using GraphConnectEngine.Core;
using GraphConnectEngine.Node;

namespace GraphTest
{
    public class MockGraph : GraphBase
    {
        public MockGraph(NodeConnector connector) : base(connector)
        {
        }

        protected override bool OnProcessCall(ProcessCallArgs args, out object[] results, out OutProcessNode nextNode)
        {
            results = Array.Empty<object>();
            nextNode = OutProcessNode;
            return true;
        }

        public override string GetGraphName()
        {
            return "MockGraph";
        }
    }
}