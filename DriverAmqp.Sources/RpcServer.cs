using System;
using System.Threading;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace DriverAmqp.Sources
{
    public class RpcServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IModel channel;
        private EventingBasicConsumer consumer;

        public delegate string Handler(string mensage);
        public event Handler HandlerMessage;

        public string response;
        
        /// <summary>
        /// RPCServer class handle the external request rabbitmq
        /// </summary>
        /// <param name="connection"></param>
        public RpcServer()
        {
            
        }

        public void Init()
        {

        }

        public void Start()
        {
            log.Info("Starting Rpc Server!");

            if (WrapperConnection.GetAMQPConnection() != null)
            {
                channel = WrapperConnection.GetAMQPConnection().CreateModel();
                consumer = new EventingBasicConsumer(channel);
                var queue = channel.QueueDeclare();
                channel.QueueBind(queue.QueueName, Util.config_rabbitmq.amqp.exchange,
                    $"{Util.config_rabbitmq.amqp.baseRoutingKey}.WatchdogPedidosProcessos.Json");
                channel.BasicConsume(queue: queue.QueueName, autoAck: false, consumer: consumer);
                log.Info(" [x] Awaiting RPC requests ");

                

                consumer.Received += (model, ea) =>
                {
                    log.Info(" Request Received ");
                    //ResponseCommand response = null;
                    //string response;

                    var body = ea.Body;

                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
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
                        channel.BasicPublish(
                            exchange: Util.config_rabbitmq.amqp.exchange,
                            routingKey: props.ReplyTo,
                            basicProperties: replyProps,
                            body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
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
        public void Stop()
		{
            
            consumer.OnCancel();
            channel.Close();
            channel.Dispose();
            
		}
    }
}
