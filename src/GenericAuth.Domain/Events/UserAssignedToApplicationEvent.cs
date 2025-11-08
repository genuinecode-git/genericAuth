using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class UserAssignedToApplicationEvent : BaseDomainEvent
{
    public Guid ApplicationId { get; }
    public Guid UserId { get; }
    public Guid ApplicationRoleId { get; }

    public UserAssignedToApplicationEvent(Guid applicationId, Guid userId, Guid applicationRoleId)
    {
        ApplicationId = applicationId;
        UserId = userId;
        ApplicationRoleId = applicationRoleId;
    }
}
