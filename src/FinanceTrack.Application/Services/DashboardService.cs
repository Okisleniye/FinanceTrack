using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IBudgetService _budgetService;

    public DashboardService(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        IBudgetService budgetService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _budgetService = budgetService ?? throw new ArgumentNullException(nameof(budgetService));
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        var totalIncome = await _context.Transactions
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var totalExpenses = await _context.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var currentMonthIncome = await _context.Transactions
            .Where(t => t.Type == TransactionType.Income && t.Date >= startOfMonth && t.Date <= endOfMonth)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var currentMonthExpenses = await _context.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startOfMonth && t.Date <= endOfMonth)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var recentTransactions = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(5)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Description = t.Description,
                Date = t.Date,
                Type = t.Type,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : string.Empty,
                CategoryColor = t.Category != null ? t.Category.Color : "#0d6efd",
                CategoryIcon = t.Category != null ? t.Category.Icon : "bi-tag",
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Group expenses by category for current month
        var monthExpenseTransactions = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startOfMonth && t.Date <= endOfMonth)
            .ToListAsync(cancellationToken);

        var spendingByCategory = monthExpenseTransactions
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var first = g.First();
                var sum = g.Sum(x => x.Amount);
                var pct = currentMonthExpenses > 0 ? Math.Round((sum / currentMonthExpenses) * 100, 1) : 0;
                return new CategorySpendingDto
                {
                    CategoryId = g.Key,
                    CategoryName = first.Category?.Name ?? "Uncategorized",
                    Color = first.Category?.Color ?? "#0d6efd",
                    Icon = first.Category?.Icon ?? "bi-tag",
                    TotalAmount = sum,
                    Percentage = pct
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        var budgets = await _budgetService.GetAllAsync(now.Year, now.Month, cancellationToken);

        return new DashboardSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            CurrentMonthIncome = currentMonthIncome,
            CurrentMonthExpenses = currentMonthExpenses,
            RecentTransactions = recentTransactions,
            SpendingByCategory = spendingByCategory,
            BudgetOverview = budgets
        };
    }
}
