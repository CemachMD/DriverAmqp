using System;
using Xunit;

namespace DriverAmqp.Sources.Tests
{
    public class RpcClientTest
    {
        [Fact]
        public void RpcClientRun()
        {
            //Arrange
            var amqp = WrapperConnection.GetInstance();
            var ch = amqp.CreateChannel();
            var rpcServer = new RpcServer(ch);
            rpcServer.Init();
            rpcServer.Start();
            var result = rpcServer.IsRunning();

            //Act

            //Assert
            Assert.True(result);
        }
    }
}
