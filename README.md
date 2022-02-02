# DriverAmqp

Implementing in .NET Standard 2.1 for compatibility with .NET Core and .NET Framework

## Configuring a Subscriber

### Create Connection
Example:

```csharp
// Load Config using static tool
var amqpConfig = Util.LoadAmqpConfig();

//Get instance and set the config
var amqp = WrapperConnection.GetInstance();
amqp.SetConfig = amqpConfig;

//Try to connect
amqp.Connect();
```

```csharp
//Define the basic parameters
exchange = "API.SQL";
routinKey = "routingKeyPubSub";

//Create a subscriber using a amqp object
var sub = new Subscriber();
sub.SetConnection = amqp.GetConnection;
sub.SetExchange = exchange;
sub.AddRoutingKey( routinKey);
sub.HandlerMessage += Sub_HandlerMessage;

//Starting process
sub.Init();
sub.Start();
sub.Listen();

private void Sub_HandlerMessage(string mensage)
{
  message = JsonConvert.DeserializeObject<Message>(mensage);
  
  //Handling the message ...
  
}

private class Message
{
  public string nome { get; set; }
  public string idade { get; set; }
 }

```


## Configuring Pattern RpcServer

### Create Connection

Example:

```csharp

string exchange = "API.SQL";
string routingKey = "api.RpcServerTest";

// Load Config using static tool
var amqpConfig = Util.LoadAmqpConfig();

//Get instance and set the config
var amqp = WrapperConnection.GetInstance();
amqp.SetConfig = amqpConfig;

//Try to connect
amqp.Connect();
```
### Configuring RpcServer Class
```csharp
//Creating the RpcServer instance
var rpcServer = new RpcServer(amqp.GetConnection);

//Set the RabbitMQ Parameters
rpcServer.SetExchange = exchange;
rpcServer.AddRoutingKey(routingKey);
//Set the handler
rpcServer.HandlerMessageWithArgs += RpcServer_HandlerMessage;


private static void RpcServer_HandlerMessage(object sender, EventArgs e)
{
  //Parser to specific object class
  var rpcServer = sender as RpcServer;
  var ea = e as BasicDeliverEventArgs;
  
  //Handler the delivery message
  var replyProps = rpcServer.CreateBasicProperties();
  replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

  var messageString = Encoding.UTF8.GetString(ea.Body.ToArray());
  var message = JsonConvert.DeserializeObject<Message>(messageString);
  Console.WriteLine(message.nome);
  Console.WriteLine(message.idade);

  //Return the message to the client
  string messageReturn = "ok";
  rpcServer.Publish(messageReturn, ea.BasicProperties.ReplyTo, replyProps, ea.DeliveryTag);
             
}

private class Message
{
  public string nome { get; set; }
  public string idade { get; set; }
}

```

The class model of configuration File
```csharp
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
```

Json File
```json
{
  "hostName": "localhost",
  "userName": "anaq",
  "password": "anaq",
  "virtualHost": null,
  "amqp": {
    "bindings": [],
    "baseRoutingKey":"TESTE",
    "exchange": "TESTE.EXCHANGE",
    "queue": {
      "name": "",
      "options": {
        "durable": true,
        "autoDelete": false
      }
    }
  }
}
```
