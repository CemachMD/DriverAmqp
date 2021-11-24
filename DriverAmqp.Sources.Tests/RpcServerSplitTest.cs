using System;
using Xunit;
using System.Collections.Generic;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;

namespace DriverAmqp.Sources.Tests
{
    public class RpcServerSplitTest
    {
        WrapperConnection amqp;
        RpcServer rpcServer;

        string exchange, routingKey;
        public RpcServerSplitTest()
        {
            exchange = "API.SQL";
            routingKey = "api.RpcServerTest";
            rpcServer = new RpcServer();
        }

        [Fact]
        public void RpcServerSplitRun()
        {
            //Arrange
            var amqpConfig = Util.LoadAmqpConfig();
            var amqp = WrapperConnection.GetInstance();
            amqp.SetConfig = amqpConfig;
            amqp.Connect();


            rpcServer.SetConnection = amqp.GetConnection;
            rpcServer.SetExchange = exchange;
            rpcServer.AddRoutingKey(routingKey);
            rpcServer.HandlerMessageWithArgs += RpcServer_HandlerMessage;
            rpcServer.Init();
            rpcServer.Listen();
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
            Assert.Equal("ok", resultrpc);
        }

        private static void RpcServer_HandlerMessage(object sender, EventArgs e)
        {
            var rpcServer = sender as RpcServer;
            var replyProps = rpcServer.CreateBasicProperties();

            var ea = e as BasicDeliverEventArgs;

            replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            rpcServer.Publish("ok", ea.BasicProperties.ReplyTo, replyProps, ea.DeliveryTag);
          
            var msg = JsonConvert.DeserializeObject<Message>(message);
            Console.WriteLine(msg.nome);
            Console.WriteLine(msg.idade);

            

        }

        private class Message
        {
            public string nome { get; set; }
            public string idade { get; set; }
        }
    }
}
