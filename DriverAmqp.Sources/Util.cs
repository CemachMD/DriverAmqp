using System;
using System.IO;
using Newtonsoft.Json;


namespace DriverAmqp.Sources
{
    public static class Util
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string basePath = AppDomain.CurrentDomain.BaseDirectory;
		private static readonly string pathRabbitmqConfig = @"amqpConfig.json";
		private static readonly string bufferSubscriberNameFile = @"BufferSubscriber.json";

		public static AmqpConfig amqpConfig;
		public static string BasePath { get => basePath; }
		public static string BufferSubscriberFilePath { get => Path.Combine(basePath,bufferSubscriberNameFile); }

		public static void LoadAmqpConfig()
		{
			log.Info($"Reading Amqp Config File: {Path.Combine(basePath, pathRabbitmqConfig)}");
			//Console.WriteLine("Reading rabbimq config: {0}", Path.Combine(basePath, pathRabbitmqConfig));
			if (File.Exists(Path.Combine(basePath, pathRabbitmqConfig)))
			{
				using (StreamReader str = new StreamReader(Path.Combine(basePath, pathRabbitmqConfig)))
				{
					string json = str.ReadToEnd();

					try
					{
						amqpConfig = JsonConvert.DeserializeObject<AmqpConfig>(json);


						if (amqpConfig.virtualHost == "")
						{
							amqpConfig.virtualHost = null;
						}
					}
					catch (Exception e)
					{
						throw e;
					}
				}
			}	
			else
			{
				log.Error("Error: Config Rabbitmq .Json doesn't exist !!");
				amqpConfig = new AmqpConfig() { amqp = new Amqp() };
				amqpConfig.hostName = "localhost";
				amqpConfig.amqp.exchange = "amq.topic";
				amqpConfig.amqp.baseRoutingKey = "amq.topic";
				amqpConfig.amqp.bindings = new System.Collections.Generic.List<string>();
				amqpConfig.amqp.bindings.Add("request");
				SaveJsonFile(amqpConfig);
			}
		}

		public static bool CheckAMQPConfigFIle()
        {
			return File.Exists(Path.Combine(basePath, pathRabbitmqConfig));
        }
		public static void SaveJsonFile(object data)
		{
			var path = Path.Combine(basePath, pathRabbitmqConfig);

			SaveJsonFile(data, path);
		}

		public static void SaveJsonFile(object data, string pathFile)
		{
			if (File.Exists(pathFile)) File.Delete(pathFile);

			using (StreamWriter file = File.CreateText(pathFile))
			{
				JsonSerializer serializer = new JsonSerializer();
				//serialize object directly into file stream	
				serializer.Formatting = Formatting.Indented;
				serializer.Serialize(file, data);
			}
		}
		public static void SaveFile(string data, string pathFile)
        {
			using (StreamWriter writer = new StreamWriter(pathFile))
            {
				writer.Write(data);
            }
        }
	}
}
