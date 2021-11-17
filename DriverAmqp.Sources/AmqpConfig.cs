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
		public string hostName { get; set; }
		public string userName { get; set; }
		public string password { get; set; }
		public string virtualHost { get; set; }
		public Amqp amqp { get; set; }

	}
	public class Amqp
	{
		public List<string> bindings { get; set; }
		public string baseRoutingKey { get; set; }
		public string exchange { get; set; }
		public Queue queue { get; set; }
	}
	public class Queue
	{
		public string name { get; set; }
		public Options options { get; set; }
	}
	public class Options
	{
		public bool durable { get; set; }
		public bool autoDelete { get; set; }
	}
}
