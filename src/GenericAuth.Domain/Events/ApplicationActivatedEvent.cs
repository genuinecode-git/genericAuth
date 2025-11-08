using GenericAuth.Domain.Common;

namespace GenericAuth.Domain.Events;

public sealed class ApplicationActivatedEvent : BaseDomainEvent
{
    public Guid ApplicationId { get; }
    public string ApplicationCode { get; }

    public ApplicationActivatedEvent(Guid applicationId, string applicationCode)
    {
        ApplicationId = applicationId;
        ApplicationCode = applicationCode;
    }
}
