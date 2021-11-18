# DriverAmqp

# Configuring Pattern RpcServer

## Connection

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
