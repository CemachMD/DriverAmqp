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
        WrapperConnection amqp;
        public PubSubTest()
        {
            amqp = WrapperConnection.GetInstance();
        }

        [Fact]
        public void Run()
        {
            //Arrange
            var chPub = amqp.CreateChannel();
            var chSub = amqp.CreateChannel();

            var sub = new Subscriber(chSub);
            sub.HandlerMessage += Sub_HandlerMessage;


            var pub = new Publisher(chPub);
            

            //Act
            sub.Init();
            sub.Start();
            sub.Listen();

            pub.Init();
            pub.Start();
            pub.Send(JsonConvert.SerializeObject(new Message() { nome = "cesae", idade = "23" }));

            //Assert

        }

        private void Sub_HandlerMessage(string mensage)
        {
            throw new NotImplementedException();
        }

        private class Message
        {
            public string nome { get; set; }
            public string idade { get; set; }
        }
    }
}
