using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class UserRemovedFromApplicationEvent : BaseDomainEvent
{
    public Guid ApplicationId { get; }
    public Guid UserId { get; }

    public UserRemovedFromApplicationEvent(Guid applicationId, Guid userId)
    {
        ApplicationId = applicationId;
        UserId = userId;
    }
}
