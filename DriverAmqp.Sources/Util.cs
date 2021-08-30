using System;
using System.IO;
using Newtonsoft.Json;


namespace DriverAmqp.Sources
{
    public static class Util
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string basePath = AppDomain.CurrentDomain.BaseDirectory;
		private static readonly string pathRabbitmqConfig = @"config_rabbitmq.json";

		public static ConfigRabbitmq config_rabbitmq;

		public static void ReadJsonRabbitmqConfig()
		{
			log.Info($"Reading rabbimq config: {Path.Combine(basePath, pathRabbitmqConfig)}");
			//Console.WriteLine("Reading rabbimq config: {0}", Path.Combine(basePath, pathRabbitmqConfig));
			if (File.Exists(Path.Combine(basePath, pathRabbitmqConfig)))
			{
				using (StreamReader str = new StreamReader(Path.Combine(basePath, pathRabbitmqConfig)))
				{
					string json = str.ReadToEnd();

					try
					{
						config_rabbitmq = JsonConvert.DeserializeObject<ConfigRabbitmq>(json);


						if (config_rabbitmq.virtualHost == "")
						{
							config_rabbitmq.virtualHost = null;
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
				
			}
		}

		public static bool CheckAMQPConfigFIle()
        {
			return File.Exists(Path.Combine(basePath, pathRabbitmqConfig));
        }
		public static void SaveJsonFile(object item)
		{
			var path = Path.Combine(basePath, pathRabbitmqConfig);

			if (File.Exists(path))
				File.Delete(path);
			
			using (StreamWriter file = File.CreateText(path))
			{
				JsonSerializer serializer = new JsonSerializer();
				//serialize object directly into file stream	
				serializer.Formatting = Formatting.Indented;
				serializer.Serialize(file, item);
			}
		}
	}
}
