using System;
using Xunit;

namespace DriverAmqp.Sources.Tests
{
    public class RpcServerTest
    {
        string exchange, routingKey;
        public RpcServerTest()
        {
            exchange = "API.SQL";
            routingKey = "api.RpcServerTest";
        }

        [Fact]
        public void RpcServerRun()
        {
            //Arrange
            var amqpConfig = Util.LoadAmqpConfig();
            var amqp = WrapperConnection.GetInstance();
            amqp.SetConfig = amqpConfig;
            amqp.Connect();
            var rpcServer = new RpcServer(amqp.GetConnection);
            rpcServer.SetExchange = exchange;
            rpcServer.AddRoutingKey(routingKey);


            //Act
            rpcServer.Init();
            rpcServer.Start();
            var result = rpcServer.IsRunning();
            //Assert
            Assert.True(result);
        }
    }
}
