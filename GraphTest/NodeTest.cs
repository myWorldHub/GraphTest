using GraphConnectEngine.Node;
using Xunit;

namespace GraphTest
{
    public class NodeTest
    {
        /// <summary>
        /// ProcessNodeの接続テスト
        /// </summary>
        [Fact]
        public void ConnectProcessNodeTest()
        {
            NodeConnector connector = new NodeConnector();
            MockGraph graph = new MockGraph(connector);

            InProcessNode inProcessNode1 = new InProcessNode(graph);
            InProcessNode inProcessNode2 = new InProcessNode(graph);
            OutProcessNode outProcessNode1 = new OutProcessNode(graph);

            //自分自身には繋げない
            Assert.False(connector.ConnectNode(inProcessNode1, inProcessNode1));
            Assert.False(connector.ConnectNode(outProcessNode1, outProcessNode1));
            
            //繋ぐ
            Assert.True(connector.ConnectNode(inProcessNode1,outProcessNode1));
            
            //確認
            Assert.True(connector.IsConnected(inProcessNode1,outProcessNode1));
            
            //もう一度試す
            Assert.False(connector.ConnectNode(inProcessNode1,outProcessNode1));
            
            //別のを繋いでみる
            Assert.False(connector.ConnectNode(inProcessNode1, new OutProcessNode(graph)));
            Assert.True(connector.ConnectNode(outProcessNode1, new InProcessNode(graph)));
            
            //切断
            Assert.True(connector.DisconnectNode(inProcessNode1,outProcessNode1));
            
            //確認
            Assert.False(connector.IsConnected(inProcessNode1,outProcessNode1));
            
            //もう一度試す
            Assert.False(connector.DisconnectNode(inProcessNode1,outProcessNode1));
            
            //再接続
            Assert.True(connector.ConnectNode(inProcessNode1,outProcessNode1));
            Assert.False(connector.DisconnectNode(inProcessNode1,new OutProcessNode(graph)));
            
            //もう一つ繋ぐ
            Assert.True(connector.ConnectNode(outProcessNode1,inProcessNode2));
            
            //確認
            Assert.True(connector.IsConnected(outProcessNode1, inProcessNode2));
            
            //全て切断
            Assert.True(connector.DisconnectAllNode(outProcessNode1));
            
            //確認
            Assert.False(connector.IsConnected(outProcessNode1,inProcessNode1));
            Assert.False(connector.IsConnected(outProcessNode1,inProcessNode2));
            
            //TODO Itemとつなげないことを確認 void & int
        }
    }
}