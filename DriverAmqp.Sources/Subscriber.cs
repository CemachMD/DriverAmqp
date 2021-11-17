using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace DriverAmqp.Sources
{
    public class Subscriber
    {

        private IConnection _conn;
        private IModel _channel;


        public delegate void Handler(string mensage);
        public event Handler HandlerMessage;

        private string _exchange;

        private List<string> _bindings;

        private bool _saveFileBuffer = false;
        public bool SaveFileBuffer { 
            get { return _saveFileBuffer; } 
            set { _saveFileBuffer = value; } 
        }

        public Subscriber()
        {
            _bindings = new List<string>();
        }

        public Subscriber(IConnection connection)
        {
            _bindings = new List<string>();
            _conn = connection;
        }

        public Subscriber(IConnection connection, string exchange)
        {
            _bindings = new List<string>();
            _conn = connection;
            _exchange = exchange;
        }
        public Subscriber(IConnection connection, string exchange, string routingKey)
        {
            _bindings = new List<string>();
            this._conn = connection;
            this._exchange = exchange;
            _bindings.Add( routingKey);
        }

        /// <summary>
        /// Set a active connection to the RabbitMQ
        /// </summary>
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

        public void AddRoutingKey(string routingKey)
        {
            _bindings.Add(routingKey);
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

        public void Close()
        {
            _channel.Close();
        }

        public void Listen()
        {
            if (_conn != null)
            {
                
                
                if (_channel != null)
                    try
                    {
                        var consumer = new EventingBasicConsumer(this._channel);
                        var queue = _channel.QueueDeclare();

                        foreach(var bind in _bindings)
                        {
                            _channel.QueueBind(queue.QueueName, _exchange, bind);
                        }
                        

                        _channel.BasicConsume(queue: queue.QueueName, autoAck: true, consumer: consumer);

                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;

                            var props = ea.BasicProperties;
                            var replyProps = _channel.CreateBasicProperties();
                            replyProps.CorrelationId = props.CorrelationId;

                            try
                            {
                                string message = Encoding.UTF8.GetString(body.ToArray());

                                if (_saveFileBuffer)
                                {
                                    Util.SaveFile(message, Util.BufferSubscriberFilePath);
                                }

                                HandlerMessage(message);

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(" [.] " + e.Message);

                            }
                            

                        };
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
    }
}
