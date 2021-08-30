using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace DriverAmqp.Sources
{
    public class Subscriber
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IModel channel;

        public delegate void Handler(string mensage);
        public event Handler HandlerMessage;

        public Subscriber()
        {

        }

        public void Init()
        {
            this.channel = WrapperConnection.GetAMQPConnection().CreateModel();
        }

        public void Start()
        {

        }

        public void Close()
        {
            this.channel.Close();
        }

        public void Listen()
        {
            if (WrapperConnection.GetAMQPConnection() != null)
            {
                
                if (this.channel != null)
                    try
                    {
                        var consumer = new EventingBasicConsumer(this.channel);
                        var queue = channel.QueueDeclare();

                        this.channel.QueueBind(queue.QueueName, Util.config_rabbitmq.amqp.exchange,
                            $"{Util.config_rabbitmq.amqp.baseRoutingKey}.StreamDataEstadosEquipamento.Json");

                        this.channel.BasicConsume(queue: queue.QueueName, autoAck: true, consumer: consumer);

                        log.Info(" [x] Awaiting RPC requests ");

                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;

                            var props = ea.BasicProperties;
                            var replyProps = channel.CreateBasicProperties();
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
                            this.channel = WrapperConnection.GetAMQPConnection().CreateModel();
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
