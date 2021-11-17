using System;
using System.Threading;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace DriverAmqp.Sources
{
    public class RpcServer
    {

        private IModel _channel;
        private EventingBasicConsumer consumer;
        private IConnection _conn;

        public delegate string Handler(string body);
        public event Handler HandlerMessage;

        public delegate void HandlerWithArgs(object sender, EventArgs args);
        public event HandlerWithArgs HandlerMessageWithArgs;

        private string _exchange;
        private List<string> _bindings;
        private bool _isRunning = false;

        public string response;
        
        /// <summary>
        /// RPCServer class handle the external request rabbitmq
        /// </summary>
        /// <param name="connection"></param>
        public RpcServer()
        {
            _bindings = new List<string>();

        }

        public RpcServer(IConnection connection)
        {
            _conn = connection;
            _bindings = new List<string>();
        }

        public RpcServer(IConnection connection , string exchange)
        {
            _bindings = new List<string>();
            _conn = connection;
            _exchange = exchange;
        }
        /// <summary>
        /// Set a active connection to the RabbitMQ
        /// </summary>
        public IConnection SetConnection { 
            set {
                if( _conn == null )
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


        public RpcServer(IConnection connection, string exchange, string routingKey)
        {
            _bindings = new List<string>();
            _conn = connection;
            _exchange = exchange;
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

            if (_conn != null)
            {
                         
                consumer = new EventingBasicConsumer(_channel);
                
                var queue = _channel.QueueDeclare();

                foreach(var bing in _bindings)
                {
                    _channel.QueueBind(queue.QueueName, _exchange, bing);
                }
                
                _channel.BasicConsume(queue: queue.QueueName, autoAck: false, consumer: consumer);
                _isRunning = true;
                
                consumer.Received += (model, ea) =>
                {
                    
                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var s = ea.RoutingKey;
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        string bodyStr = Encoding.UTF8.GetString(body.ToArray());

                        response = HandlerMessage(bodyStr);
                      
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);

                        _channel.BasicPublish(
                            exchange: _exchange,
                            routingKey: props.ReplyTo,
                            basicProperties: replyProps,
                            body: responseBytes);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag,
                        multiple: false);

                    }
                };
            }
            else
            {
                Thread.Sleep(1000);
                _isRunning = false;
                Start();
            }           
        }

        public void Listen()
        {
            if (_conn != null)
            {

                consumer = new EventingBasicConsumer(_channel);

                var queue = _channel.QueueDeclare();

                foreach (var bing in _bindings)
                {
                    _channel.QueueBind(queue.QueueName, _exchange, bing);
                }

                _channel.BasicConsume(queue: queue.QueueName, autoAck: false, consumer: consumer);
                _isRunning = true;

                consumer.Received += (model, ea) =>
                {

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var s = ea.RoutingKey;
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        string bodyStr = Encoding.UTF8.GetString(body.ToArray());

                         HandlerMessageWithArgs(this, ea);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                };
            }
        }

        public IBasicProperties CreateBasicProperties()
        {
            return _channel.CreateBasicProperties();
        }

        public void Publish(string data, string replyTo, IBasicProperties replyProps, ulong deliveryTag)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);

            _channel.BasicPublish(
                            exchange: _exchange,
                            routingKey: replyTo,
                            basicProperties: replyProps,
                            body: dataBytes);

            _channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
        }

        public void NoAck(ulong deliveryTag)
        {
            _channel.BasicNack(deliveryTag: deliveryTag, multiple: false, requeue:true);
        }

        public bool IsRunning()
        {
            return _isRunning;
        }
        public void Stop()
		{
            
            consumer.OnCancel();
            _channel.Close();
            _channel.Dispose();
            
		}
    }
}
