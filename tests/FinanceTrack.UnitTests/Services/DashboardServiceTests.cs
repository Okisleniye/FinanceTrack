using FinanceTrack.Application.Services;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.UnitTests.Common;
using Xunit;

namespace FinanceTrack.UnitTests.Services;

public class DashboardServiceTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly BudgetService _budgetService;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _dateTimeProvider = new TestDateTimeProvider
        {
            UtcNow = new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc)
        };
        _budgetService = new BudgetService(_context, _dateTimeProvider);
        _service = new DashboardService(_context, _dateTimeProvider, _budgetService);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldCalculateDashboardMetricsAccurately()
    {
        // Arrange
        var salaryCat = new Category("Salary", TransactionType.Income);
        var groceryCat = new Category("Groceries", TransactionType.Expense);
        var rentCat = new Category("Rent", TransactionType.Expense);
        _context.Categories.AddRange(salaryCat, groceryCat, rentCat);
        await _context.SaveChangesAsync();

        // Previous month transactions
        _context.Transactions.AddRange(
            new Transaction(500000m, "October Salary", new DateTime(2026, 10, 25), TransactionType.Income, salaryCat.Id),
            new Transaction(150000m, "October Rent", new DateTime(2026, 10, 26), TransactionType.Expense, rentCat.Id)
        );

        // Current month (November 2026) transactions
        _context.Transactions.AddRange(
            new Transaction(600000m, "November Salary", new DateTime(2026, 11, 5), TransactionType.Income, salaryCat.Id),
            new Transaction(50000m, "Supermarket 1", new DateTime(2026, 11, 7), TransactionType.Expense, groceryCat.Id),
            new Transaction(30000m, "Supermarket 2", new DateTime(2026, 11, 10), TransactionType.Expense, groceryCat.Id),
            new Transaction(150000m, "November Rent", new DateTime(2026, 11, 1), TransactionType.Expense, rentCat.Id)
        );

        // Budget for current month
        _context.Budgets.Add(new Budget(100000m, 11, 2026, groceryCat.Id));
        await _context.SaveChangesAsync();

        // Act
        var summary = await _service.GetSummaryAsync();

        // Assert
        // Total historical metrics:
        // Income = 500k + 600k = 1,100,000
        // Expenses = 150k + 50k + 30k + 150k = 380,000
        // Balance = 1,100,000 - 380,000 = 720,000
        Assert.Equal(1100000m, summary.TotalIncome);
        Assert.Equal(380000m, summary.TotalExpenses);
        Assert.Equal(720000m, summary.TotalBalance);

        // Current month metrics:
        // Income = 600k
        // Expenses = 50k + 30k + 150k = 230,000
        // Balance = 600k - 230k = 370,000
        Assert.Equal(600000m, summary.CurrentMonthIncome);
        Assert.Equal(230000m, summary.CurrentMonthExpenses);
        Assert.Equal(370000m, summary.CurrentMonthBalance);

        // Recent transactions: at most 5 items
        Assert.Equal(5, summary.RecentTransactions.Count);

        // Category spending:
        // Rent = 150k (65.2%), Groceries = 80k (34.8%)
        Assert.Equal(2, summary.SpendingByCategory.Count);
        Assert.Equal("Rent", summary.SpendingByCategory[0].CategoryName);
        Assert.Equal(150000m, summary.SpendingByCategory[0].TotalAmount);
        Assert.Equal(65.2m, summary.SpendingByCategory[0].Percentage);

        Assert.Equal("Groceries", summary.SpendingByCategory[1].CategoryName);
        Assert.Equal(80000m, summary.SpendingByCategory[1].TotalAmount);
        Assert.Equal(34.8m, summary.SpendingByCategory[1].Percentage);

        // Budget overview:
        Assert.Single(summary.BudgetOverview);
        Assert.Equal(80000m, summary.BudgetOverview[0].AmountSpent);
    }

    [Fact]
    public async Task GetSummaryAsync_WithEmptyDatabase_ShouldReturnZeroesAndEmptyLists()
    {
        // Act
        var summary = await _service.GetSummaryAsync();

        // Assert
        Assert.Equal(0m, summary.TotalIncome);
        Assert.Equal(0m, summary.TotalExpenses);
        Assert.Equal(0m, summary.TotalBalance);
        Assert.Equal(0m, summary.CurrentMonthIncome);
        Assert.Equal(0m, summary.CurrentMonthExpenses);
        Assert.Equal(0m, summary.CurrentMonthBalance);
        Assert.Empty(summary.RecentTransactions);
        Assert.Empty(summary.SpendingByCategory);
        Assert.Empty(summary.BudgetOverview);
    }
}
