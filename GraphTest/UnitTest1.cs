using GraphConnectEngine.Core;
using GraphConnectEngine.Graph;
using Xunit;
using Xunit.Abstractions;

namespace GraphTest
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            
            GraphEngineLogger.LogLevel = 0;
            GraphEngineLogger.SetLogMethod(_testOutputHelper.WriteLine);
        }

        [Fact]
        public void Test1()
        {
            int count = 0;

            NodeConnector connector = new NodeConnector();

            UpdaterGraph updater = new UpdaterGraph(connector);
            updater.IntervalType = UpdaterGraph.Type.Update;

            IntGraph intGraph = new IntGraph(connector);

            DebugTextGraph textGraph = new DebugTextGraph(connector, msg =>
            {
                var result = msg == intGraph.Number.ToString();
                _testOutputHelper.WriteLine($"{msg} : {count} : {intGraph.Number} : {result}");
                Assert.True(result);
            });

            //connect
            Assert.True(connector.ConnectNode(updater.OutProcessNode, textGraph.InProcessNode));
            Assert.True(connector.ConnectNode(intGraph.OutItemNode, textGraph.InItemNode));

            //Assert
            for (; count < 10; count++)
            {
                updater.Update(0);
                intGraph.Number++;
            }

            //disconnect
            Assert.True(connector.DisconnectNode(updater.OutProcessNode, textGraph.InProcessNode));
            Assert.True(connector.DisconnectNode(intGraph.OutItemNode, textGraph.InItemNode));
        }

        [Fact]
        public void Test2()
        {
            for (int i = 0; i < 100; i++)
            {
                _testOutputHelper.WriteLine("----------------------------" + i);
                
                int count = 0;
                string variableName = "SampleInt";

                NodeConnector connector = new NodeConnector();

                VariableHolder holder = new VariableHolder();
                Assert.True(holder.TryCreateItem(variableName, -1));
                Assert.False(holder.TryCreateItem(variableName, -1));

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                IntGraph intGraph = new IntGraph(connector);
                intGraph.Number = 1;

                AddGraph addGraph = new AddGraph(connector);

                GetVariableGraph getVariableGraph1 = new GetVariableGraph(connector, holder);
                getVariableGraph1.VariableName = variableName;

                SetVariableGraph setVariableGraph = new SetVariableGraph(connector, holder);
                setVariableGraph.VariableName = variableName;

                DebugTextGraph textGraph = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine(msg + " : " + count);
                    Assert.Equal(count.ToString(), msg);
                });

                GetVariableGraph getVariableGraph2 = new GetVariableGraph(connector, holder);
                getVariableGraph2.VariableName = variableName;

                //connect
                Assert.True(connector.ConnectNode(updater.OutProcessNode, setVariableGraph.InProcessNode));
                Assert.True(connector.ConnectNode(setVariableGraph.OutProcessNode, textGraph.InProcessNode));

                Assert.True(connector.ConnectNode(addGraph.InItemNode1, intGraph.OutItemNode));
                Assert.True(connector.ConnectNode(addGraph.InItemNode2, getVariableGraph1.OutItemNode));
                Assert.True(connector.ConnectNode(addGraph.OutItemNode, setVariableGraph.InItemNode));

                Assert.True(connector.ConnectNode(getVariableGraph2.OutItemNode, textGraph.InItemNode));

                //Assert
                for (; count < 10; count++)
                {
                    updater.Update(0);
                }
            }
        }
    }
}