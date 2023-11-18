using Newtonsoft.Json;
using PersonTrackApi.Models;
using PersonTrackApi.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQEventBus;
using RabbitMQEventBus.Constants;
using RabbitMQEventBus.Events;
using System.Text;

namespace PersonTrackApi.Consumer;

public class PersonMovementReportEventBusConsumer
{
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly PersonMovementService _personMovementService;

    public PersonMovementReportEventBusConsumer(IRabbitMQPersistentConnection persistentConnection, PersonMovementService personMovementService)
    {
        _persistentConnection = persistentConnection;
        _personMovementService = personMovementService;
    }

    public void Consume()
    {
        if (_persistentConnection.IsConnected == false)
            _persistentConnection.TryConnect();

        var channel = _persistentConnection.CreateModel();
        channel.QueueDeclare(queue: EventConstants.PersonMovementReportQueue, durable: true, false, false, null);
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += Consumer_Received;
        channel.BasicConsume(EventConstants.PersonMovementReportQueue, true, consumer);
    }

    private async void Consumer_Received(object? sender, BasicDeliverEventArgs e)
    {
        var body = e.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        if (e.RoutingKey == EventConstants.PersonMovementReportQueue)
        {
            var @reportCreateEvent = JsonConvert.DeserializeObject<PersonMovementCreateEvent>(message);
            await PersonReportProcess(@reportCreateEvent);
        }
    }
    public void Disconnect()
    {
        _persistentConnection.Dispose();
    }

    private async Task PersonReportProcess(PersonMovementCreateEvent model)
    {

        PersonMovement personMovement = new()
        {
            PersonId = model.PersonId,
            MovementType = model.MovementType,
            MovementTypeName = model.MovementTypeName,
            CreatedAt = model.CreatedAt,
        };

        _ = await _personMovementService.AddPersonMovementProcess(personMovement);
    }
}
