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
        [Fact]
        public void Run()
        {
            //Arrange
            var amqp = WrapperConnection.GetInstance();

            var chPub = amqp.CreateChannel();
            var chSub = amqp.CreateChannel();

            var sub = new Subscriber(chSub);
            sub.HandlerMessage += Sub_HandlerMessage;
            sub.Init();
            sub.Start();
            sub.Listen();

            var pub = new Publisher(chPub);
            pub.Init();
            pub.Start();
            pub.Send(JsonConvert.SerializeObject(new Message() {nome="cesae",idade="23" }));

            //Act
            sub.Init();
            sub.Start();
            sub.Listen();



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
