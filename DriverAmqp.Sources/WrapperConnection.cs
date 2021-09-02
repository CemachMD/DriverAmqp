﻿using System;
using System.Threading;
using RabbitMQ.Client;

namespace DriverAmqp.Sources
{
    public class WrapperConnection
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static WrapperConnection _instance;
        private static readonly object _lock = new ();
        private readonly ConnectionFactory factory;
        private static IConnection conn;


        private WrapperConnection()
        {
            if(Util.amqpConfig!=null)
            {
                this.factory = new ConnectionFactory()
                {
                    HostName = Util.amqpConfig.hostName,
                    UserName = Util.amqpConfig.userName,
                    Password = Util.amqpConfig.password,
                    VirtualHost = Util.amqpConfig.virtualHost,
                };
                if (Util.amqpConfig.virtualHost == null) factory.VirtualHost = "/";
            }
            else
            {
                this.factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                };
            }
                

            factory.AutomaticRecoveryEnabled = true;
            factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
            TryConnect();
            /*
            Thread tryConnect = new Thread(TryConnect);
            tryConnect.Start();
            */
            
        }

        private void TryConnect()
        {
            try
            {
                factory.RequestedConnectionTimeout = TimeSpan.FromSeconds(5);
                conn = this.factory.CreateConnection();
                log.Info("Connection created Successfully!");
            }
            catch(Exception e)
            {
                log.Error("Error to connect RabbitMQ: " + e.Message);
                Thread.Sleep(1000);
                TryConnect();
            }
        }

        public static IConnection GetAMQPConnection()
        {
            //log.Info("Getting AMQP Connection");

            return conn;
        }

        public static WrapperConnection GetInstance()
        {
            if(_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        Util.LoadAmqpConfig();
                        _instance = new WrapperConnection();
                }
            }

            return _instance;
        }

        public static void Close()
        {
            conn.Close();
        }

        public IModel CreateChannel()
        {
            return conn.CreateModel();
        }
        
    }
}
