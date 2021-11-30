using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace DriverAmqp.Sources
{
    public class Subscriber : BaseController
    {

        public delegate void Handler(string mensage);
        public event Handler HandlerMessage;

        public delegate void HandlerWithArgs(object sender, EventArgs args);
        public event HandlerWithArgs HandlerMessageWithArgs;

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
        /// Add the routing key to list for binding with the current exchange
        /// </summary>
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
                        QueueDeclareOk queue;

                        if (_queue != null && _queue != "")
                        {
                            queue = _channel.QueueDeclare(_queue, durable: true, autoDelete: false, exclusive: false);
                        }
                        else
                            queue = _channel.QueueDeclare();

                        //Binding
                        foreach (var bind in _bindings)
                        {
                            _channel.QueueBind(queue.QueueName, _exchange, bind);
                        }
                        

                        _channel.BasicConsume(queue: queue.QueueName, autoAck: true, consumer: consumer);

                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;

                            try
                            {
                                string message = Encoding.UTF8.GetString(body.ToArray());

                                if (_saveFileBuffer)
                                {
                                    Util.SaveFile(message, Util.BufferSubscriberFilePath);
                                }

                                HandlerMessage(message);
                                HandlerMessageWithArgs(this, ea);

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
