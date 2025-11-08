using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class ApplicationRoleSetAsDefaultEvent : BaseDomainEvent
{
    public Guid RoleId { get; }
    public Guid ApplicationId { get; }
    public string RoleName { get; }

    public ApplicationRoleSetAsDefaultEvent(Guid roleId, Guid applicationId, string roleName)
    {
        RoleId = roleId;
        ApplicationId = applicationId;
        RoleName = roleName;
    }
}
