using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class UserRegisteredEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserRegisteredEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
