namespace RabbitMQEventBus.Events.Interfaces;

public abstract class IEvent
{
    public Guid RequestId { get; protected set; }
    public DateTime Timestamp { get; protected set; }


    protected IEvent()
    {
        RequestId = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }
}