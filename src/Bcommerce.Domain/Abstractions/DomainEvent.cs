namespace Bcommerce.Domain.Abstractions;

public class DomainEvent
{
    public DateTime OccuredOn { get; set; }
    protected DomainEvent()
    {
        OccuredOn = DateTime.Now;
    }
}