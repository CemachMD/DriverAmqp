using System;
using System.Threading;
using RabbitMQ.Client;

namespace DriverAmqp.Sources
{
    public class WrapperConnection
    {

        private static WrapperConnection _instance;
        private static readonly object _lock = new object();
        private ConnectionFactory factory;
        private static IConnection _conn;
        private static AmqpConfig _amqpConfig;

        private WrapperConnection()
        {          

        }

        private void CreateFactory()
        {
            this.factory = new ConnectionFactory()
            {
                HostName = _amqpConfig.hostName,
                UserName = _amqpConfig.userName,
                Password = _amqpConfig.password,
                
            };
            if (_amqpConfig.virtualHost != null)
                factory.VirtualHost = _amqpConfig.virtualHost; 

            factory.AutomaticRecoveryEnabled = true;
            factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
        }

        public void Connect()
        {
            CreateFactory();
            TryConnect();
            
        }
        private void TryConnect()
        {
            try
            {
                factory.RequestedConnectionTimeout = TimeSpan.FromSeconds(5);
                _conn = this.factory.CreateConnection();
                Console.WriteLine("Connection created Successfully!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error to connect RabbitMQ: " + e.Message);
                Thread.Sleep(5000);
                TryConnect();
            }
        }

        /// <summary>
        /// Return the current Amqp Connectio to the RabbitMQ
        /// </summary>
        public IConnection GetConnection
        {
            get
            {
                lock (_lock)
                {
                    return _conn;
                }
            }
        }

        public AmqpConfig SetConfig
        {
            set
            {
                _amqpConfig = value;
            }
        }

        public static WrapperConnection GetInstance()
        {
            if(_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new WrapperConnection();
                }
            }

            return _instance;
        }

        public void Close()
        {
            _conn.Close();
        }

        public IModel CreateChannel()
        {
            return _conn.CreateModel();
        }

        public bool IsConnected()
        {
            return _conn.IsOpen;
        }
        
    }
}
