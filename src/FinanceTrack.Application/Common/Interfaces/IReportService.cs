using FinanceTrack.Application.DTOs;

namespace FinanceTrack.Application.Common.Interfaces;

public interface IReportService
{
    Task<FinancialReportDto> GetReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyTrendDto>> GetMonthlyTrendsAsync(int monthsCount = 6, CancellationToken cancellationToken = default);
}
