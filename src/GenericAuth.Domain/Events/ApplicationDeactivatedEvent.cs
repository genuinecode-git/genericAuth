using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class ApplicationDeactivatedEvent : BaseDomainEvent
{
    public Guid ApplicationId { get; }
    public string ApplicationCode { get; }

    public ApplicationDeactivatedEvent(Guid applicationId, string applicationCode)
    {
        ApplicationId = applicationId;
        ApplicationCode = applicationCode;
    }
}
