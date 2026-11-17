using FinanceTrack.Application.DTOs;

namespace FinanceTrack.Application.Common.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
