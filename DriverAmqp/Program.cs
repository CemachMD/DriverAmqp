using System;
using DriverAmqp.Sources;
using Newtonsoft.Json;

namespace DriverAmqp
{
    class Program
    {
        static void Main(string[] args)
        {

            var amqpConfig = Util.LoadAmqpConfig();
            var amqp = WrapperConnection.GetInstance();
            amqp.SetConfig = amqpConfig;
            Console.WriteLine("Connecting");
            amqp.Connect();
            Console.WriteLine("Connected");


            var rpcServer = new RpcServer();
            rpcServer.SetConnection = amqp.GetConnection;
            rpcServer.SetExchange = "API.SQL";
            rpcServer.AddRoutingKey("api.RpcServerTest");
            rpcServer.SetQueue = "q_TESTE_SQL";
            rpcServer.HandlerMessage += RpcServer_HandlerMessage;
            rpcServer.Init();
            rpcServer.Start();

            Console.WriteLine("Iniciado");


            var chClient = amqp.CreateChannel();
            var rpcClient = new RpcClient(chClient, "API.SQL", "api.RpcServerTest");
            rpcClient.Start();
            var data = new Message() { nome = "Cesar", idade = "19" };
            var resultrpc = rpcClient.Call<Message>(data, "API.SQL", "api.RpcServerTest");
            Console.WriteLine(resultrpc);

            Console.ReadKey();
        }

        private static string RpcServer_HandlerMessage(string mensage)
        {
            var msg = JsonConvert.DeserializeObject<Message>(mensage);
            Console.WriteLine(msg.nome);
            Console.WriteLine(msg.idade);
            return "ok";

        }

        private class Message
        {
            public string nome { get; set; }
            public string idade { get; set; }
        }
    }
}
