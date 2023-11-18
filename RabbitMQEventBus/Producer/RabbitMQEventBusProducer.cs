using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client.Exceptions;
using RabbitMQEventBus.Events.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace RabbitMQEventBus.Producer;

public class RabbitMQEventBusProducer : IRabbitMQEventBusProducer
{
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly ILogger<RabbitMQEventBusProducer> _logger;
    private readonly int? _retryCount;

    public RabbitMQEventBusProducer(ILogger<RabbitMQEventBusProducer> logger, IRabbitMQPersistentConnection persistentConnection, int? retryCount = 5)
    {
        _logger=logger;
        _persistentConnection=persistentConnection;
        _retryCount=retryCount;
    }

    public void Publish(string queueName, IEvent @event)
    {
        if (_persistentConnection.IsConnected == false)
            _persistentConnection.TryConnect();

        //polling policy
        var policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry((int)_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex.ToString());
                    });

        _logger.LogInformation("RabbitMQ persistent connection acquired a connection");
        using var channel = _persistentConnection.CreateModel();
        var eventName = @event.GetType().Name;
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);

        //policy publish için eklendi, eğer publish başarısız olursa tekrar dener
        policy.Execute(() =>
        {
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.DeliveryMode = 2; // persistent
            channel.ConfirmSelect();
            channel.BasicPublish(exchange: "", routingKey: queueName, mandatory: true, basicProperties: properties, body: body);
            channel.WaitForConfirmsOrDie();
            channel.BasicAcks += (sender, eventArgs) =>
            {
                _logger.LogInformation("Sent RabbitMQ");
            };
            channel.ConfirmSelect();
        });
    }
}