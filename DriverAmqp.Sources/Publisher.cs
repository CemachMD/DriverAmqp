using System.Text;
using System;
using RabbitMQ.Client;

namespace DriverAmqp.Sources
{
    public class Publisher
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IModel _channel;
        private string _exchange, _routingKey;
        public Publisher()
        {
            
        }
        public Publisher(IModel channel)
        {
            this._channel = channel;
        }
        public Publisher(IModel channel, string exchange)
        {
            this._channel = channel;
            this._exchange = exchange;
        }
        public Publisher(IModel channel, string exchange, string routingKey)
        {
            this._channel = channel;
            this._exchange = exchange;
            this._routingKey = routingKey;
        }

        public void Init()
        {
            if(this._channel==null)
                this._channel = WrapperConnection.GetAMQPConnection().CreateModel();
            if (this._exchange == null) _exchange = Util.amqpConfig.amqp.exchange;
            if (this._routingKey == null)
                _routingKey = $"{Util.amqpConfig.amqp.baseRoutingKey}.{Util.amqpConfig.amqp.bindings[0]}";
        }

        public void Start()
        {

        }

        public void Send(string message)
        {
            Send(message, _exchange, _routingKey);
        }

        public void Send(string message,string exchange, string routingKey)
        {
            if(WrapperConnection.GetAMQPConnection() != null)
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                if (this._channel != null)
                    try
                    {
                        this._channel.BasicPublish(exchange, routingKey, basicProperties: null, body: messageBytes);
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
                            this._channel = WrapperConnection.GetAMQPConnection().CreateModel();
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
            this._channel.Close();
        }
    }
}
