using System;
using Xunit;
using Newtonsoft.Json;

namespace DriverAmqp.Sources.Tests
{
    public class RpcClientTest
    {
        [Fact]
        public void RpcClientRun()
        {
            //Arrange
            var amqp = WrapperConnection.GetInstance();
            var chServer = amqp.CreateChannel();
            var rpcServer = new RpcServer(chServer,"API.SQL","api.routingkey");
            rpcServer.HandlerMessage += RpcServer_HandlerMessage;
            rpcServer.Init();
            rpcServer.Start();
            var result = rpcServer.IsRunning();



            //Act
            var chClient = amqp.CreateChannel();
            var rpcClient = new RpcClient(chClient, "API.SQL", "api.routingkey");
            rpcClient.Start();
            var data = new Message() { nome = "Cesar", idade = "19" };
            var resultrpc = rpcClient.Call<Message>(data, "API.SQL", "api.routingkey");
            Console.WriteLine(resultrpc);

            //Assert
            Assert.True(result);
            Assert.Equal("ok",resultrpc );
        }

        private string RpcServer_HandlerMessage(string mensage)
        {
            var msg = JsonConvert.DeserializeObject<Message>(mensage);
            Console.WriteLine(msg.nome);
            Console.WriteLine(msg.idade);
            return "ok";

        }

        public class Message
        {
            public string nome { get; set; }
            public string idade { get; set; }
        }
    }
}
