using GenericAuth.Application.Common.Interfaces;

namespace GenericAuth.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
