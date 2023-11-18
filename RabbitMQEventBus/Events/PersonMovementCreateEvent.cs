using RabbitMQEventBus.Events.Interfaces;

namespace RabbitMQEventBus.Events;

public class PersonMovementCreateEvent : IEvent
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int MovementType { get; set; } //Enter = 1, Exit = 2
    public string MovementTypeName { get; set; } //Enter = 1, Exit = 2
    public DateTime CreatedAt { get; set; }
}

