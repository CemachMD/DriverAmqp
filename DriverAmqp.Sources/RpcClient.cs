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
        private readonly  IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;   

        public RpcClient()
        {
			var factory = new ConnectionFactory()
			{
				HostName = Util.config_rabbitmq.hostName,
				UserName = Util.config_rabbitmq.userName,
				Password = Util.config_rabbitmq.password,
                VirtualHost=Util.config_rabbitmq.virtualHost,
			};
            if (Util.config_rabbitmq.virtualHost == null)
            {
                factory.VirtualHost = "/";
            }

            connection = factory.CreateConnection();
			channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);   

            channel.QueueBind(replyQueueName, Util.config_rabbitmq.amqp.exchange, replyQueueName);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
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
            var resp = "";            
            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            channel.BasicPublish(
                exchange: Util.config_rabbitmq.amqp.exchange,
                routingKey: $"{Util.config_rabbitmq.amqp.baseRoutingKey}.WatchdogPedidosProcessos.Json",
                basicProperties: this.props,
                body: messageBytes);
            channel.BasicConsume(
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
            channel.Close();
            connection.Close();
        }

    }
}
