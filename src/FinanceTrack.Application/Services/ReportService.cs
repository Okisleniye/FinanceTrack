using System.Globalization;
using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.Application.Services;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReportService(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<FinancialReportDto> GetReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        if (startDate > endDate)
        {
            throw new ApplicationValidationException("Start date cannot be greater than end date.", nameof(startDate));
        }

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToListAsync(cancellationToken);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var first = g.First();
                var sum = g.Sum(t => t.Amount);
                var pct = totalExpenses > 0 ? Math.Round((sum / totalExpenses) * 100, 1) : 0;
                return new CategorySpendingDto
                {
                    CategoryId = g.Key,
                    CategoryName = first.Category?.Name ?? "Uncategorized",
                    Color = first.Category?.Color ?? "#dc3545",
                    Icon = first.Category?.Icon ?? "bi-tag",
                    TotalAmount = sum,
                    Percentage = pct
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        var incomeByCategory = transactions
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var first = g.First();
                var sum = g.Sum(t => t.Amount);
                var pct = totalIncome > 0 ? Math.Round((sum / totalIncome) * 100, 1) : 0;
                return new CategorySpendingDto
                {
                    CategoryId = g.Key,
                    CategoryName = first.Category?.Name ?? "Uncategorized",
                    Color = first.Category?.Color ?? "#28a745",
                    Icon = first.Category?.Icon ?? "bi-cash",
                    TotalAmount = sum,
                    Percentage = pct
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        // Calculate trends across months in period
        var monthlyTrends = transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g =>
            {
                var inc = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
                var exp = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
                var date = new DateTime(g.Key.Year, g.Key.Month, 1);
                return new MonthlyTrendDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = date.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Income = inc,
                    Expenses = exp
                };
            })
            .ToList();

        return new FinancialReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            ExpensesByCategory = expensesByCategory,
            IncomeByCategory = incomeByCategory,
            MonthlyTrends = monthlyTrends
        };
    }

    public async Task<IReadOnlyList<MonthlyTrendDto>> GetMonthlyTrendsAsync(int monthsCount = 6, CancellationToken cancellationToken = default)
    {
        var count = Math.Clamp(monthsCount, 1, 24);
        var now = _dateTimeProvider.UtcNow;
        var startMonthDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(count - 1));

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Date >= startMonthDate)
            .ToListAsync(cancellationToken);

        var result = new List<MonthlyTrendDto>(count);

        for (var i = 0; i < count; i++)
        {
            var targetDate = startMonthDate.AddMonths(i);
            var year = targetDate.Year;
            var month = targetDate.Month;

            var inc = transactions
                .Where(t => t.Type == TransactionType.Income && t.Date.Year == year && t.Date.Month == month)
                .Sum(t => t.Amount);

            var exp = transactions
                .Where(t => t.Type == TransactionType.Expense && t.Date.Year == year && t.Date.Month == month)
                .Sum(t => t.Amount);

            result.Add(new MonthlyTrendDto
            {
                Year = year,
                Month = month,
                MonthName = targetDate.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Income = inc,
                Expenses = exp
            });
        }

        return result;
    }
}
