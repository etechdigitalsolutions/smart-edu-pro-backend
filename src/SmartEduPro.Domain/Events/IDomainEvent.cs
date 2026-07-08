namespace SmartEduPro.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
