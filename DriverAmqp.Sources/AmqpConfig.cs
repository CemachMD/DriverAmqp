using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace DriverAmqp.Sources
{
	public class AmqpConfig
	{
		public string hostName;
		public string userName;
		public string password;
		public string virtualHost;
		public Amqp amqp;

	}
	public class Amqp
	{
		public List<string> bindings;
		public string baseRoutingKey;
		public string exchange;
		public Queue queue;
	}
	public class Queue
	{
		public string name;
		public Options options;
	}
	public class Options
	{
		public bool durable;
		public bool autoDelete;
	}
}
