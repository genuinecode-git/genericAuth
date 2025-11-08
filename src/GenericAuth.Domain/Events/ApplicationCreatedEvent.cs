using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class ApplicationCreatedEvent : BaseDomainEvent
{
    public Guid ApplicationId { get; }
    public string ApplicationCode { get; }

    public ApplicationCreatedEvent(Guid applicationId, string applicationCode)
    {
        ApplicationId = applicationId;
        ApplicationCode = applicationCode;
    }
}
