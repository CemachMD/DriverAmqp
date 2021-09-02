using System;
using DriverAmqp.Sources;

namespace DriverAmqp
{
    class Program
    {
        static void Main(string[] args)
        {

            //var amqp = WrapperConnection.GetInstance();

            //var ch = amqp.CreateChannel();

            var rpcServer = new RpcServer();
            rpcServer.HandlerMessage += RpcServer_HandlerMessage;
            rpcServer.Init();
            rpcServer.Start();

            Console.WriteLine("Iniciado");
            Console.ReadKey();
        }

        private static string RpcServer_HandlerMessage(string mensage)
        {
            throw new NotImplementedException();
        }
    }
}
