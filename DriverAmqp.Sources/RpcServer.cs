using System;
using System.Threading;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DriverAmqp.Sources
{

    public class RpcServer<T> : RpcServer
    {
        public delegate Task HandlerTaskWithArgs(object sender, EventArgs args);
        public event HandlerTaskWithArgs HandlerTaskMessageWithArgs;

        public override void Listen()
        {
            if (_conn != null)
            {

                consumer = new EventingBasicConsumer(_channel);

                QueueDeclareOk queue;

                if (_queue != null && _queue != "")
                {
                    queue = _channel.QueueDeclare(_queue, durable: true, autoDelete: false, exclusive: false);
                }
                else
                    queue = _channel.QueueDeclare();

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

                        HandlerTaskMessageWithArgs(this, ea);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error to HandlerTaskMessageWithArgs :: " + ex.Message);
                    }

                };
            }
        }
    }

    public class RpcServer : BaseController
    {

        protected EventingBasicConsumer consumer;
        protected bool _isRunning = false;

        public delegate string Handler(string body);
        public event Handler HandlerMessage;

        public delegate void HandlerWithArgs(object sender, EventArgs args);
        public virtual event HandlerWithArgs HandlerMessageWithArgs;      

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



        public RpcServer(IConnection connection, string exchange, string routingKey)
        {
            _bindings = new List<string>();
            _conn = connection;
            _exchange = exchange;
            _bindings.Add(routingKey);
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
            catch (Exception ex)
            {
                throw new Exception("Error to Create Channel :: " + ex.Message);
            }
        }

        public void Start()
        {

            if (_conn != null)
            {
                         
                consumer = new EventingBasicConsumer(_channel);
                QueueDeclareOk queue;
                
                if(_queue !=null && _queue != "")
                {
                    queue = _channel.QueueDeclare(_queue, durable: true, autoDelete:false, exclusive:false);
                }
                else
                    queue = _channel.QueueDeclare();

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
                        throw new Exception("Error to HandlerMessage :: " + ex.Message);
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

        public virtual void Listen()
        {
            if (_conn != null)
            {

                consumer = new EventingBasicConsumer(_channel);

                QueueDeclareOk queue;

                if (_queue != null && _queue != "")
                {
                    queue = _channel.QueueDeclare(_queue, durable: true, autoDelete: false, exclusive: false);
                }
                else
                    queue = _channel.QueueDeclare();

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
                        throw new Exception("Error to HandlerMessageWithArgs :: " + ex.Message);
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

        public void Publish(string data, string exchange, string replyTo, IBasicProperties replyProps, ulong deliveryTag)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);

            _channel.BasicPublish(
                            exchange: exchange,
                            routingKey: replyTo,
                            basicProperties: replyProps,
                            body: dataBytes);

            _channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
        }

        public void Ack(ulong deliveryTag)
        {
            _channel.BasicAck(deliveryTag: deliveryTag, multiple: true);
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
