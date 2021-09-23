using System;
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
        public void Test1()
        {
            for (int i = 0; i < 100; i++)
            {
                int count = 0;

                NodeConnector connector = new NodeConnector();
                
                _testOutputHelper.WriteLine("connector" + (connector.ToString()));

                UpdaterGraph updater = new UpdaterGraph(connector);
                updater.IntervalType = UpdaterGraph.Type.Update;

                ValueGraph<int> intGraph = new ValueGraph<int>(connector,0);

                DebugTextGraph textGraph = new DebugTextGraph(connector, msg =>
                {
                    var result = msg == intGraph.Value.ToString();
                    _testOutputHelper.WriteLine($"{msg} : {count} : {intGraph.Value} : {result}");
                    Assert.True(result);
                    return true;
                });

                //connect
                Assert.True(connector.ConnectNode(updater.OutProcessNode, textGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.OutItemNodes[0], textGraph.InItemNodes[0]));

                //Assert
                for (; count < 100; count++)
                {
                    updater.Update(0);
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
        public void Test2()
        {
            for (int i = 0; i < 100; i++)
            {
                _testOutputHelper.WriteLine("----------------------------" + i);
                
                int count = 0;
                string variableName = "SampleInt";

                NodeConnector connector = new NodeConnector();

                VariableHolder holder = new VariableHolder();
                Assert.True(holder.TryCreateItem(variableName, 0));
                Assert.False(holder.TryCreateItem(variableName, 0));

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
                    return true;
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
                    updater.Update(0);
                }
            }
        }

        /// <summary>
        /// intグラフにプロセスを通す版 (Test1) 
        /// updater => int => text
        /// </summary>
        [Fact]
        public void Test3()
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
                    return true;
                });


                Assert.True(connector.ConnectNode(updater.OutProcessNode, intGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.OutProcessNode, textGraph.InProcessNode));
                Assert.True(connector.ConnectNode(intGraph.OutItemNodes[0], textGraph.InItemNodes[0]));

                //Assert
                for (; count < 100; count++)
                {
                    updater.Update(0);
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
        public void Test4()
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
                    return true;
                });

                Assert.True(connector.ConnectNode(updater.OutProcessNode, equal.InProcessNode));
                Assert.True(connector.ConnectNode(equal.OutProcessNode, text.InProcessNode));

                Assert.True(connector.ConnectNode(equal.InItemNodes[0], intGraph.OutItemNodes[0]));
                Assert.True(connector.ConnectNode(equal.InItemNodes[1], intGraph.OutItemNodes[0]));

                Assert.True(connector.ConnectNode(text.InItemNodes[0], equal.OutItemNodes[2]));

                for (int j = 0; j < 10; j++)
                {
                    updater.Update(0);
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
        public void Test5()
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
                    return true;
                });

                Assert.True(connector.ConnectNode(updater.OutProcessNode, text.InProcessNode));

                Assert.True(connector.ConnectNode(equal.InItemNodes[0], intGraph.OutItemNodes[0]));
                Assert.True(connector.ConnectNode(equal.InItemNodes[1], intGraph.OutItemNodes[0]));

                Assert.True(connector.ConnectNode(text.InItemNodes[0], equal.OutItemNodes[2]));

                for (int j = 0; j < 10; j++)
                {
                    updater.Update(0);
                }
            }
        }


        /// <summary>
        /// updater   => if text
        /// int=>equal=>
        /// </summary>
        [Fact]
        public void Test6()
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
                    return true;
                });

                DebugTextGraph text2 = new DebugTextGraph(connector, msg =>
                {
                    _testOutputHelper.WriteLine("Assert : " + msg);
                    Assert.Equal("False", msg);
                    return true;
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
                    updater.Update(0);
                }
            }
        }
    }
}