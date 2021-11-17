using System;
using Xunit;
using Newtonsoft.Json;

namespace DriverAmqp.Sources.Tests
{
    public class RpcClientTest
    {
        WrapperConnection amqp;
        string exchange, routingKey;

        public RpcClientTest()
        {
            var amqpConfig = Util.LoadAmqpConfig();
            amqp = WrapperConnection.GetInstance();
            amqp.SetConfig = amqpConfig;
            amqp.Connect();
            exchange = "API.SQL";
            routingKey = "api.routingkey";
        }
        [Fact]
        public void RpcClientRun()
        {
            //Arrange
            
            var rpcServer = new RpcServer();
            rpcServer.SetConnection = amqp.GetConnection;
            rpcServer.SetExchange = exchange;
            rpcServer.AddRoutingKey( routingKey);
            rpcServer.HandlerMessage += RpcServer_HandlerMessage;
            rpcServer.Init();
            rpcServer.Start();
            var result = rpcServer.IsRunning();
            Console.WriteLine(result);


            //Act
            var chClient = amqp.CreateChannel();
            var rpcClient = new RpcClient(chClient, exchange, routingKey);
            rpcClient.Start();
            var data = new Message() { nome = "Cesar", idade = "19" };
            var resultrpc = rpcClient.Call<Message>(data, exchange, routingKey);
            Console.WriteLine(resultrpc);

            System.Threading.Thread.Sleep(2000);

            //Assert
            Assert.True(result);
            Assert.Equal("ok",resultrpc );
        }

        private static string RpcServer_HandlerMessage(string mensage)
        {
            var msg = JsonConvert.DeserializeObject<Message>(mensage);
            Console.WriteLine(msg.nome);
            Console.WriteLine(msg.idade);
            return "ok";

        }

        private class Message
        {
            public string nome { get; set; }
            public string idade { get; set; }
        }
    }
}
