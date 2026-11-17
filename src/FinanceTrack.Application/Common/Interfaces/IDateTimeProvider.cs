namespace FinanceTrack.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
