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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IModel _channel;
        private EventingBasicConsumer consumer;

        public delegate string Handler(string mensage);
        public event Handler HandlerMessage;

        private string exchange;
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
            var amqp = WrapperConnection.GetInstance();
            _channel = amqp.CreateChannel();
        }

        public RpcServer(IModel channel)
        {
            _bindings = new List<string>();
            this._channel = channel;


        }

        public RpcServer(IModel channel , string exchange)
        {
            _bindings = new List<string>();
            this._channel = channel;
            this.exchange = exchange;


        }
        public RpcServer(IModel channel, string exchange, string routingKey)
        {
            _bindings = new List<string>();
            this._channel = channel;
            this.exchange = exchange;
            _bindings.Add(routingKey);
        }

        public void Init()
        { 

            if (this._channel == null) 
            {
                var amqp = WrapperConnection.GetInstance();
                _channel = amqp.CreateChannel();
            }

            if (this.exchange == null) this.exchange = Util.amqpConfig.amqp.exchange;
            if (Util.amqpConfig != null)
            {
                foreach(var bing in Util.amqpConfig.amqp.bindings)
                {
                    var routing = $"{Util.amqpConfig.amqp.baseRoutingKey}.{bing}";
                    _bindings.Add(routing);
                }
            }
        }

        public void Start()
        {
            log.Info("Starting Rpc Server!");

            if (WrapperConnection.GetAMQPConnection() != null)
            {
                
                _channel.ExchangeDeclare(this.exchange, ExchangeType.Topic, true, false,null);

                consumer = new EventingBasicConsumer(_channel);
                
                var queue = _channel.QueueDeclare();

                foreach(var bing in _bindings)
                {
                    _channel.QueueBind(queue.QueueName, this.exchange, bing);
                }
                
                _channel.BasicConsume(queue: queue.QueueName, autoAck: false, consumer: consumer);
                _isRunning = true;
                log.Info(" [x] Awaiting RPC requests ");
                
                consumer.Received += (model, ea) =>
                {
                    log.Info(" Request Received ");
                    //ResponseCommand response = null;
                    //string response;

                    var body = ea.Body;

                    var props = ea.BasicProperties;
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        string message = Encoding.UTF8.GetString(body.ToArray());
                        log.Info($"Request: {message}");
                        response = HandlerMessage(message);

                        log.Info($"Response: {response}");
                    }
                    catch (Exception e)
                    {
                        log.Error(" [.] " + e.Message);
                        //response.error = true;
                    }
                    finally
                    {

                        //var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                        var responseBytes = Encoding.UTF8.GetBytes(response);

                        //var responseBytes = Encoding.UTF8.GetBytes(response);
                        log.Info($"ReplyTo: {props.ReplyTo}");
                        _channel.BasicPublish(
                            exchange: this.exchange,
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
                Start();
            }

            
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
