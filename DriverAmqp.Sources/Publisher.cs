using System.Text;
using System;
using RabbitMQ.Client;

namespace DriverAmqp.Sources
{
    public class Publisher
    {
        private IModel _channel;
        private string _exchange, _routingKey;
        private IConnection _conn;
        public Publisher()
        {
            
        }
        public Publisher(IConnection connection)
        {
            _conn = connection;
        }
        public Publisher(IConnection connection, string exchange)
        {
            _conn = connection;
            _exchange = exchange;
        }
        public Publisher(IConnection connection, string exchange, string routingKey)
        {
            _conn = connection;
            _exchange = exchange;
            _routingKey = routingKey;
        }

        public IConnection SetConnection
        {
            set
            {
                if (_conn == null)
                    _conn = value;
            }
        }

        /// <summary>
        /// Set a name of the durable topic Exchange
        /// </summary>
        public string SetExchange
        {
            set
            {
                if (_exchange != value)
                    _exchange = value;
            }
        }

        /// <summary>
        /// Set a name the Routing Key using to bind with the Exchange
        /// </summary>
        public string SetRoutingKey
        {
            set
            {
                if (_routingKey != value)
                    _routingKey = value;
            }
        }

        public void Init()
        {
            try
            {
                _channel = _conn.CreateModel();
                _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, true, false, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Start()
        {

        }

        public void Publish(string message)
        {
            Publish(message, _exchange, _routingKey);
        }

        public void Publish(string message,string exchange, string routingKey)
        {
            if(_conn != null)
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                if (this._channel != null)
                    try
                    {
                        this._channel.BasicPublish(exchange, routingKey, basicProperties: null, body: messageBytes);
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine(e);
                    }
                    
                else
                {
                    try
                    {
                        if (_conn != null)
                            this._channel = _conn.CreateModel();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
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
