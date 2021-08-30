using System.Text;
using System;
using RabbitMQ.Client;

namespace DriverAmqp.Sources
{
    public class Publisher
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IModel channel;
        public Publisher()
        {
            
        }

        public void Init()
        {
            this.channel = WrapperConnection.GetAMQPConnection().CreateModel();
        }

        public void Start()
        {

        }
        public void Send(string message,string exchange, string routingKey)
        {
            if(WrapperConnection.GetAMQPConnection() != null)
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                if (this.channel != null)
                    try
                    {
                        this.channel.BasicPublish(exchange, routingKey, basicProperties: null, body: messageBytes);
                    }
                    catch (Exception e)
                    {

                        log.Error("Error Publisher Sending: "+e.Message);
                    }
                    
                else
                {
                    try
                    {
                        if (WrapperConnection.GetAMQPConnection() != null)
                            this.channel = WrapperConnection.GetAMQPConnection().CreateModel();
                    }
                    catch (Exception e)
                    {
                        log.Error("Error Publisher: " + e.Message);
                    }
                }
            }
            
                
        }
        public void Close()
        {
            this.channel.Close();
        }
    }
}
