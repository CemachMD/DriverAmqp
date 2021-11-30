using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace DriverAmqp.Sources
{
    public abstract class BaseController
    {
        protected IConnection _conn;
        protected IModel _channel;
        protected string _exchange,_routingKey;
        protected List<string> _bindings;
        protected string _queue;

        /// <summary>
        /// Set a active connection to the RabbitMQ
        /// </summary>
        public IConnection SetConnection
        {
            set
            {
                if (_conn == null)
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

        public string SetQueue
        {
            set
            {
                if (_queue != value)
                    _queue = value;
            }
        }
    }
}
