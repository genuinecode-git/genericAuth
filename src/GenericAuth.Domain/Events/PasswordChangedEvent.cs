using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class PasswordChangedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public PasswordChangedEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
