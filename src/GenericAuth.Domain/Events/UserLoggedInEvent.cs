using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class UserLoggedInEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserLoggedInEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
