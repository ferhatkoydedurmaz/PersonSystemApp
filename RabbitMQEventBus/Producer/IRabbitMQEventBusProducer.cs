using RabbitMQEventBus.Events.Interfaces;

namespace RabbitMQEventBus.Producer;

public interface IRabbitMQEventBusProducer
{
    void Publish(string queueName, IEvent @event);
}