using RabbitMQ.Client;
using System;

namespace DriverAmqp.Sources
{
    public class DeliveryArgs : EventArgs
    {
        public IBasicProperties BasicProperties { get; set; }
        public string ConsumerTag { get; set; }
        //
        // Resumo:
        //     The delivery tag for this delivery. See IModel.BasicAck.
        public ulong DeliveryTag { get; set; }
        //
        // Resumo:
        //     The exchange the message was originally published to.
        public string Exchange { get; set; }
        //
        // Resumo:
        //     The AMQP "redelivered" flag.
        public bool Redelivered { get; set; }
        //
        // Resumo:
        //     The routing key used when the message was originally published.
        public string RoutingKey { get; set; }
    }
}
