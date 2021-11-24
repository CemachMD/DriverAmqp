using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Threading.Tasks;


namespace DriverAmqp.Sources
{
    /// <summary>
    /// Clase RpcClient para solicitar o enviar comandos
    /// </summary>
	public class RpcClient : BaseController
	{
        private string replyQueueName;
        private EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private IBasicProperties props;


        public RpcClient(){}

        /// <summary>
        /// Set a name the Routing Key using to bind with the Exchange
        /// </summary>
        public string SetRoutingKey
        {
            set
            {
                _routingKey = value;
            }
        }

        public RpcClient(IModel channel)
        {
            _channel = channel;

        }
        public RpcClient(IModel channel, string exchange)
        {

            _channel = channel;
            _exchange = exchange;


        }
        public RpcClient(IModel channel, string exchange, string routingKey)
        {

            _channel = channel;
            _exchange = exchange;
            _routingKey = routingKey;

        }

        public void Start()
        {
            if (_channel == null)
                _channel = _conn.CreateModel();

            replyQueueName = _channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(_channel);

            _channel.QueueBind(replyQueueName, _exchange, replyQueueName);

            var correlationId = Guid.NewGuid().ToString();

            props = _channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
        }

        public string Call<T>(T message)
        {
            return Call<T>(message, _exchange, _routingKey);
        }

        public string Call<T>(T message, string exchange, string routingKey)
        {
            var resp = string.Empty;            
            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: this.props,
                body: messageBytes);
            _channel.BasicConsume(
                    consumer: this.consumer,
                    queue: this.replyQueueName,
                    autoAck: true);
           

            var task = Task.Run(() => respQueue.Take());
            if (task.Wait(TimeSpan.FromSeconds(10)))         // 10 seg para o timeout
                resp = task.Result;      
            
            //  resp = respQueue.Take();           

            return resp; 
        }

        public void Close()
        {
            _channel.Close();
        }

    }
}
