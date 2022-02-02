using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;

namespace DriverAmqp.Sources.Tests
{

    public class PubSubTest
    {
        Message message;
        WrapperConnection amqp;
        string exchange;
        string routinKey;

        public PubSubTest()
        {
            message = null;
            var amqpConfig = Util.LoadAmqpConfig();
            amqp = WrapperConnection.GetInstance();
            amqp.SetConfig = amqpConfig;
            amqp.Connect();
            exchange = "API.SQL";
            routinKey = "routingKeyPubSub";
        }

        [Fact]
        public void Run()
        {
            //Arrange

            var sub = new Subscriber();
            sub.SetConnection = amqp.GetConnection;
            sub.SetExchange = exchange;
            sub.AddRoutingKey( routinKey);
            sub.HandlerMessage += Sub_HandlerMessage;

            var pub = new Publisher();
            pub.SetConnection = amqp.GetConnection;
            pub.SetExchange = exchange;
            pub.SetRoutingKey = routinKey;

            //Act
            sub.Init();
            sub.Start();
            sub.Listen();

            pub.Init();
            pub.Start();

            var _myMessage = new Message() { nome = "cesae", idade = "23" };
            pub.Publish(JsonConvert.SerializeObject(_myMessage));

            System.Threading.Thread.Sleep(2000);
            //Assert
            Assert.Equal(_myMessage.nome, message.nome);

        }

        private void Sub_HandlerMessage(string mensage)
        {
            
            message = JsonConvert.DeserializeObject<Message>(mensage);
        }

        private class Message
        {
            public string nome { get; set; }
            public string idade { get; set; }
        }
    }
}
