using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;


namespace DriverAmqp.Sources
{
    /// <summary>
    /// Clase RpcClient para solicitar o enviar comandos
    /// </summary>
	public class RpcClient
	{
        private readonly IConnection connection;
        private readonly  IModel _channel;
        private string replyQueueName;
        private EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private IBasicProperties props;

        private readonly string _exchange, _routingKey;

        public RpcClient()
        {
            Util.LoadAmqpConfig();
			var factory = new ConnectionFactory()
			{
				HostName = Util.amqpConfig.hostName,
				UserName = Util.amqpConfig.userName,
				Password = Util.amqpConfig.password,
                VirtualHost=Util.amqpConfig.virtualHost,
			};
            if (Util.amqpConfig.virtualHost == null)
            {
                factory.VirtualHost = "/";
            }

            connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            this._exchange = Util.amqpConfig.amqp.exchange;
            this._routingKey = $"{Util.amqpConfig.amqp.baseRoutingKey}.{Util.amqpConfig.amqp.bindings[0]}";
                  
        }

        public RpcClient(IModel channel)
        {
            this._channel = channel;
            this._exchange = Util.amqpConfig.amqp.exchange;
            this._routingKey = $"{Util.amqpConfig.amqp.baseRoutingKey}.{Util.amqpConfig.amqp.bindings[0]}";

        }
        public RpcClient(IModel channel, string exchange)
        {

            this._channel = channel;
            this._exchange = exchange;
            this._routingKey = $"{Util.amqpConfig.amqp.baseRoutingKey}.{Util.amqpConfig.amqp.bindings[0]}";

        }
        public RpcClient(IModel channel, string exchange, string routingKey)
        {

            this._channel = channel;
            this._exchange = exchange;
            this._routingKey = routingKey;

        }

        public void Start()
        {
            replyQueueName = _channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(_channel);

            _channel.QueueBind(replyQueueName, this._exchange, replyQueueName);

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
            return Call<T>(message, this._exchange, this._routingKey);
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
            if(connection!=null) connection.Close();
        }

    }
}
