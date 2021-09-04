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
        public PubSubTest()
        {
            message = null;
            amqp = WrapperConnection.GetInstance();
        }

        [Fact]
        public void Run()
        {
            //Arrange
            var chPub = amqp.CreateChannel();
            var chSub = amqp.CreateChannel();

            var sub = new Subscriber(chSub, "API.SQL", "routingKeyPubSub");
            sub.HandlerMessage += Sub_HandlerMessage;

            var pub = new Publisher(chPub, "API.SQL", "routingKeyPubSub");

            //Act
            sub.Init();
            sub.Start();
            sub.Listen();

            pub.Init();
            pub.Start();

            var _myMessage = new Message() { nome = "cesae", idade = "23" };
            pub.Send(JsonConvert.SerializeObject(_myMessage));

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
