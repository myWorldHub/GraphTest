
using System.Threading.Tasks;
using GraphConnectEngine.Core;
using GraphConnectEngine.Graph;
using GraphConnectEngine.Graph.Value;
using GraphConnectEngine.Graph.Event;
using GraphConnectEngine.Graph.Operator;
using GraphConnectEngine.Graph.Statement;
using GraphConnectEngine.Graph.Variable;
using GraphConnectEngine.Node;
using Xunit;
using Xunit.Abstractions;
namespace GraphTest
{
    //TODO そもそもDebugTextが実行されるかの確認
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            
            Logger.LogLevel = 0;
            Logger.SetLogMethod(_testOutputHelper.WriteLine);
        }

        /// <summary>
        /// 最も単純な出力テスト
        /// updater => text
        ///     int =>
        /// </summary>
        [Fact]
        public async void Test1()
        {
            for (int i = 0; i < 100; i++)
            {
                int count = 0;

                NodeConnector connector = new NodeConnector();
                
                _testOutputHelper.WriteLine("connector" + (connector.ToString()));

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                ValueGraph<int> intGraph = new ValueGraph<int>(connector,0);

                DebugTextGraph textGraph = new DebugTextGraph(connector,  msg =>
                {
                    var result = msg == intGraph.Value.ToString();
                    _testOutputHelper.WriteLine($"{msg} : {count} : {intGraph.Value} : {result}");
                    Assert.True(result);
                    return Task.FromResult(true);
                });

                //connect
                Assert.True(connector.ConnectNode(updater.OutProcessNode, textGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.OutItemNodes[0], textGraph.InItemNodes[0]));

                //Assert
                for (; count < 100; count++)
                {
                    await updater.Update(0);
                    intGraph.Value++;
                }
            }
        }

        /// <summary>
        /// 変数ノードのテスト
        /// updater    => set => text
        /// get => add =>
        /// int           get =>
        /// </summary>
        [Fact]
        public async void Test2()
        {
            for (int i = 0; i < 100; i++)
            {
                _testOutputHelper.WriteLine("----------------------------" + i);
                
                int count = 0;
                string variableName = "SampleInt";

                NodeConnector connector = new NodeConnector();

                VariableHolder holder = new VariableHolder();
                Assert.True(await holder.TryCreate(variableName, 0));
                Assert.False(await holder.TryCreate(variableName, 0));

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                ValueGraph<int> intGraph = new ValueGraph<int>(connector,1);

                AdditionOperatorGraph addGraph = new AdditionOperatorGraph(connector);

                GetVariableGraph getVariableGraph1 = new GetVariableGraph(connector, holder);
                getVariableGraph1.VariableName = variableName;

                SetVariableGraph setVariableGraph = new SetVariableGraph(connector, holder);
                setVariableGraph.VariableName = variableName;

                int a = 1;
                DebugTextGraph textGraph = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg + " : " + a);
                    Assert.Equal(a.ToString(), msg);
                    a++;
                    return Task.FromResult(true);
                });

                GetVariableGraph getVariableGraph2 = new GetVariableGraph(connector, holder);
                getVariableGraph2.VariableName = variableName;

                //connect
                Assert.True(connector.ConnectNode(updater.OutProcessNode, setVariableGraph.InProcessNode));
                Assert.True(connector.ConnectNode(setVariableGraph.OutProcessNode, textGraph.InProcessNode));

                Assert.True(connector.ConnectNode(addGraph.InItemNodes[0], intGraph.OutItemNodes[0]));
                Assert.True(connector.ConnectNode(addGraph.InItemNodes[1], getVariableGraph1.OutItemNodes[0]));
                Assert.True(connector.ConnectNode(addGraph.OutItemNodes[2], setVariableGraph.InItemNodes[0]));

                Assert.True(connector.ConnectNode(getVariableGraph2.OutItemNodes[0], textGraph.InItemNodes[0]));

                //Assert
                for (; count < 100; count++)
                {
                    await updater.Update(0);
                }
            }
        }

        /// <summary>
        /// intグラフにプロセスを通す版 (Test1) 
        /// updater => int => text
        /// </summary>
        [Fact]
        public async void Test3()
        {
            for (int i = 0; i < 100; i++)
            {
                _testOutputHelper.WriteLine("----------------------------" + i);
                
                int count = 0;

                NodeConnector connector = new NodeConnector();

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                ValueGraph<int> intGraph = new ValueGraph<int>(connector,1);
                
                DebugTextGraph textGraph = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg + " : "+ intGraph.Value);
                    Assert.Equal(intGraph.Value.ToString(), msg);
                    return Task.FromResult(true);
                });


                Assert.True(connector.ConnectNode(updater.OutProcessNode, intGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.OutProcessNode, textGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.OutItemNodes[0], textGraph.InItemNodes[0]));

                //Assert
                for (; count < 100; count++)
                {
                    await updater.Update(0);
                    intGraph.Value++;
                }
            }
        }

        /// <summary>
        /// updater => equal => text
        ///     int =>
        /// アイテムのストリームと、equalグラフのテスト
        /// </summary>
        [Fact]
        public async void Test4()
        {
            for (int i = 0; i < 100; i++)
            {
                _testOutputHelper.WriteLine("----------------------------" + i);

                NodeConnector connector = new NodeConnector();

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                ValueGraph<int> intGraph = new ValueGraph<int>(connector,1);

                EqualOperatorGraph equal = new EqualOperatorGraph(connector);

                DebugTextGraph text = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg);
                    Assert.Equal("True", msg);
                    return Task.FromResult(true);
                });

                Assert.True(connector.ConnectNode(updater.OutProcessNode, equal.InProcessNode));
                Assert.True(connector.ConnectNode(equal.OutProcessNode, text.InProcessNode));

                Assert.True(connector.ConnectNode(equal.InItemNodes[0], intGraph.OutItemNodes[0]));
                Assert.True(connector.ConnectNode(equal.InItemNodes[1], intGraph.OutItemNodes[0]));

                Assert.True(connector.ConnectNode(text.InItemNodes[0], equal.OutItemNodes[2]));

                for (int j = 0; j < 10; j++)
                {
                    await updater.Update(0);
                }
            }
        }

        /// <summary>
        /// updater   => text
        /// int=>equal=>
        ///
        /// text => equalの確認
        /// </summary>
        [Fact]
        public async void Test5()
        {
            for (int i = 0; i < 100; i++)
            {
                _testOutputHelper.WriteLine("----------------------------" + i);

                NodeConnector connector = new NodeConnector();

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                ValueGraph<int> intGraph = new ValueGraph<int>(connector,1);

                EqualOperatorGraph equal = new EqualOperatorGraph(connector);

                DebugTextGraph text = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg);
                    Assert.Equal("True", msg);
                    return Task.FromResult(true);
                });

                Assert.True(connector.ConnectNode(updater.OutProcessNode, text.InProcessNode));

                Assert.True(connector.ConnectNode(equal.InItemNodes[0], intGraph.OutItemNodes[0]));
                Assert.True(connector.ConnectNode(equal.InItemNodes[1], intGraph.OutItemNodes[0]));

                Assert.True(connector.ConnectNode(text.InItemNodes[0], equal.OutItemNodes[2]));

                for (int j = 0; j < 10; j++)
                {
                    await updater.Update(0);
                }
            }
        }


        /// <summary>
        /// updater   => if text
        /// int=>equal=>
        /// </summary>
        [Fact]
        public async void Test6()
        {
            for (int i = 0; i < 100; i++)
            {
                _testOutputHelper.WriteLine("----------------------------" + i);

                NodeConnector connector = new NodeConnector();

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                ValueGraph<int> intGraph1 = new ValueGraph<int>(connector, 1);
                ValueGraph<int> intGraph2 = new ValueGraph<int>(connector, 1);

                EqualOperatorGraph equal = new EqualOperatorGraph(connector);

                IfStatementGraph ifGraph = new IfStatementGraph(connector);

                DebugTextGraph text1 = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg);
                    Assert.Equal("True", msg);
                    return Task.FromResult(true);
                });

                DebugTextGraph text2 = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg);
                    Assert.Equal("False", msg);
                    return Task.FromResult(true);
                });

                Assert.True(connector.ConnectNode(updater.OutProcessNode, ifGraph.InProcessNode));
                Assert.True(connector.ConnectNode(ifGraph.OutProcessNode, text1.InProcessNode));
                Assert.True(connector.ConnectNode(ifGraph.OutProcessNodes[1], text2.InProcessNode));

                Assert.True(connector.ConnectNode(equal.InItemNodes[0], intGraph1.OutItemNodes[0]));
                Assert.True(connector.ConnectNode(equal.InItemNodes[1], intGraph2.OutItemNodes[0]));

                Assert.True(connector.ConnectNode(ifGraph.InItemNodes[0], equal.OutItemNodes[2]));

                Assert.True(connector.ConnectNode(ifGraph.OutItemNodes[0], text1.InItemNodes[0]));
                Assert.True(connector.ConnectNode(ifGraph.OutItemNodes[0], text2.InItemNodes[0]));

                for (int j = 0; j < 10; j++)
                {
                    await updater.Update(0);
                    if (i % 2 == 0)
                    {
                        intGraph1.Value++;
                    }
                    else
                    {
                        intGraph2.Value = intGraph1.Value;
                    }
                }
            }
        }


        [Fact]
        public async void CastGraph_GeneralTest()
        {
            var conn = new NodeConnector();

            var updater = new UpdaterGraph(conn);
            var cast = new CastGraph<int>(conn);
            var fv = new ValueGraph<float>(conn, 1.1f);
            var text = new DebugTextGraph(conn, str =>
            {
                _testOutputHelper.WriteLine($"ASSERT : 1 : {str} : {fv.Value}");
                Assert.Equal("1", str);
                return Task.FromResult(true);
            });

            Assert.True(conn.ConnectNode(updater.OutProcessNode, text.InProcessNode));
            Assert.True(conn.ConnectNode(fv.OutItemNodes[0], cast.InItemNodes[0]));
            Assert.True(conn.ConnectNode(cast.OutItemNodes[0], text.InItemNodes[0]));

            await updater.Update(0);
        }
        
    }
}