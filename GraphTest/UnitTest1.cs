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
            for (int i = 0; i < 100; i++)
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
                    return true;
                });

                //connect
                Assert.True(connector.ConnectNode(updater.OutProcessNode, textGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.GetOutItemNode(0), textGraph.GetInItemNode(0)));

                //Assert
                for (; count < 100; count++)
                {
                    updater.Update(0);
                    intGraph.Number++;
                }

                //disconnect
                Assert.True(connector.DisconnectNode(updater.OutProcessNode, textGraph.InProcessNode));
                Assert.True(connector.DisconnectNode(intGraph.GetOutItemNode(0), textGraph.GetInItemNode(0)));
            }
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

                int a = 0;
                DebugTextGraph textGraph = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg + " : " + a);
                    Assert.Equal(a.ToString(), msg);
                    a++;
                    return true;
                });

                GetVariableGraph getVariableGraph2 = new GetVariableGraph(connector, holder);
                getVariableGraph2.VariableName = variableName;

                //connect
                Assert.True(connector.ConnectNode(updater.OutProcessNode, setVariableGraph.InProcessNode));
                Assert.True(connector.ConnectNode(setVariableGraph.OutProcessNode, textGraph.InProcessNode));

                Assert.True(connector.ConnectNode(addGraph.GetInItemNode(0), intGraph.GetOutItemNode(0)));
                Assert.True(connector.ConnectNode(addGraph.GetInItemNode(1), getVariableGraph1.GetOutItemNode(0)));
                Assert.True(connector.ConnectNode(addGraph.GetOutItemNode(0), setVariableGraph.GetInItemNode(0)));

                Assert.True(connector.ConnectNode(getVariableGraph2.GetOutItemNode(0), textGraph.GetInItemNode(0)));

                //Assert
                for (; count < 100; count++)
                {
                    updater.Update(0);
                }
            }
        }

        [Fact]
        public void Test3()
        {
            for (int i = 0; i < 100; i++)
            {
                _testOutputHelper.WriteLine("----------------------------" + i);
                
                int count = 0;
                string variableName = "SampleInt";

                NodeConnector connector = new NodeConnector();

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                IntGraph intGraph = new IntGraph(connector);
                intGraph.Number = 1;
                
                DebugTextGraph textGraph = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg + " : "+ intGraph.Number);
                    Assert.Equal(intGraph.Number.ToString(), msg);
                    return true;
                });


                Assert.True(connector.ConnectNode(updater.OutProcessNode, intGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.OutProcessNode, textGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.GetOutItemNode(0), textGraph.GetInItemNode(0)));

                //Assert
                for (; count < 100; count++)
                {
                    updater.Update(0);
                    intGraph.Number++;
                }
            }
        }
    }
}