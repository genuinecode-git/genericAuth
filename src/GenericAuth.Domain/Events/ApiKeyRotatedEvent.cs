using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class ApiKeyRotatedEvent : BaseDomainEvent
{
    public Guid ApplicationId { get; }
    public string ApplicationCode { get; }
    public string OldKeyHash { get; }

    public ApiKeyRotatedEvent(Guid applicationId, string applicationCode, string oldKeyHash)
    {
        ApplicationId = applicationId;
        ApplicationCode = applicationCode;
        OldKeyHash = oldKeyHash;
    }
}
