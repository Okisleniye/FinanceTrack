using FinanceTrack.Application.Common.Interfaces;

namespace FinanceTrack.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
