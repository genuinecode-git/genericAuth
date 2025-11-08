using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class UserRoleChangedInApplicationEvent : BaseDomainEvent
{
    public Guid ApplicationId { get; }
    public Guid UserId { get; }
    public Guid NewApplicationRoleId { get; }

    public UserRoleChangedInApplicationEvent(Guid applicationId, Guid userId, Guid newApplicationRoleId)
    {
        ApplicationId = applicationId;
        UserId = userId;
        NewApplicationRoleId = newApplicationRoleId;
    }
}
