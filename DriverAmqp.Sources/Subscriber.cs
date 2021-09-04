using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace DriverAmqp.Sources
{
    public class Subscriber
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IModel _channel;

        public delegate void Handler(string mensage);
        public event Handler HandlerMessage;

        private string _exchange, _routingKey;

        public Subscriber()
        {

        }

        public Subscriber(IModel channel)
        {
            this._channel = channel;
        }
        public Subscriber(IModel channel,string exchange)
        {
            this._channel = channel;
            this._exchange = exchange;
        }
        public Subscriber(IModel channel,string exchange, string routingKey)
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

        public void Close()
        {
            this._channel.Close();
        }

        public void Listen()
        {
            if (WrapperConnection.GetAMQPConnection() != null)
            {
                
                if (this._channel != null)
                    try
                    {
                        var consumer = new EventingBasicConsumer(this._channel);
                        var queue = _channel.QueueDeclare();

                        this._channel.QueueBind(queue.QueueName, _exchange,_routingKey);

                        this._channel.BasicConsume(queue: queue.QueueName, autoAck: true, consumer: consumer);

                        //log.Info(" [x] Awaiting RPC requests ");

                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;

                            var props = ea.BasicProperties;
                            var replyProps = _channel.CreateBasicProperties();
                            replyProps.CorrelationId = props.CorrelationId;

                            try
                            {
                                string message = Encoding.UTF8.GetString(body.ToArray());
                                //log.Info($"Request: {message}");
                                HandlerMessage(message);

                                //log.Info($"Response: {response}");
                            }
                            catch (Exception e)
                            {
                                log.Error(" [.] " + e.Message);
                                //response.error = true;
                            }
                            

                        };
                    }
                    catch (Exception e)
                    {

                        log.Error("Error Listen: " + e.Message);
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
    }
}
