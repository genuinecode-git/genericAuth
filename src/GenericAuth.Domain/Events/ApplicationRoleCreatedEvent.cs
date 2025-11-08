using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class ApplicationRoleCreatedEvent : BaseDomainEvent
{
    public Guid RoleId { get; }
    public Guid ApplicationId { get; }
    public string RoleName { get; }

    public ApplicationRoleCreatedEvent(Guid roleId, Guid applicationId, string roleName)
    {
        RoleId = roleId;
        ApplicationId = applicationId;
        RoleName = roleName;
    }
}
