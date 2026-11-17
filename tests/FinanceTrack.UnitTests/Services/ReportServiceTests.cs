using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.Services;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.UnitTests.Common;
using Xunit;

namespace FinanceTrack.UnitTests.Services;

public class ReportServiceTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _dateTimeProvider = new TestDateTimeProvider
        {
            UtcNow = new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc)
        };
        _service = new ReportService(_context, _dateTimeProvider);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetReportAsync_WithInvalidDateRange_ShouldThrowApplicationValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            _service.GetReportAsync(new DateTime(2026, 11, 20), new DateTime(2026, 11, 10)));
    }

    [Fact]
    public async Task GetReportAsync_ShouldCalculateReportAndCategoryBreakdownsCorrectly()
    {
        // Arrange
        var salaryCat = new Category("Salary", TransactionType.Income);
        var bonusCat = new Category("Bonus", TransactionType.Income);
        var groceriesCat = new Category("Groceries", TransactionType.Expense);
        var transportCat = new Category("Transport", TransactionType.Expense);
        _context.Categories.AddRange(salaryCat, bonusCat, groceriesCat, transportCat);
        await _context.SaveChangesAsync();

        _context.Transactions.AddRange(
            new Transaction(400000m, "Monthly Salary", new DateTime(2026, 11, 1), TransactionType.Income, salaryCat.Id),
            new Transaction(100000m, "Performance Bonus", new DateTime(2026, 11, 10), TransactionType.Income, bonusCat.Id),
            new Transaction(60000m, "Groceries 1", new DateTime(2026, 11, 5), TransactionType.Expense, groceriesCat.Id),
            new Transaction(40000m, "Groceries 2", new DateTime(2026, 11, 12), TransactionType.Expense, groceriesCat.Id),
            new Transaction(50000m, "Train Passes", new DateTime(2026, 11, 8), TransactionType.Expense, transportCat.Id)
        );
        await _context.SaveChangesAsync();

        // Act
        var report = await _service.GetReportAsync(new DateTime(2026, 11, 1), new DateTime(2026, 11, 30));

        // Assert
        // Income = 400k + 100k = 500k
        // Expenses = 60k + 40k + 50k = 150k
        // Net = 350k
        // Savings rate = (350k / 500k) * 100 = 70%
        Assert.Equal(500000m, report.TotalIncome);
        Assert.Equal(150000m, report.TotalExpenses);
        Assert.Equal(350000m, report.NetSavings);
        Assert.Equal(70.0m, report.SavingsRate);

        // Expense categories: Groceries (100k - 66.7%), Transport (50k - 33.3%)
        Assert.Equal(2, report.ExpensesByCategory.Count);
        Assert.Equal("Groceries", report.ExpensesByCategory[0].CategoryName);
        Assert.Equal(100000m, report.ExpensesByCategory[0].TotalAmount);
        Assert.Equal(66.7m, report.ExpensesByCategory[0].Percentage);

        // Income categories: Salary (400k - 80%), Bonus (100k - 20%)
        Assert.Equal(2, report.IncomeByCategory.Count);
        Assert.Equal("Salary", report.IncomeByCategory[0].CategoryName);
        Assert.Equal(400000m, report.IncomeByCategory[0].TotalAmount);
        Assert.Equal(80.0m, report.IncomeByCategory[0].Percentage);
    }

    [Fact]
    public async Task GetReportAsync_WithEmptyDatabase_ShouldReturnZeroesAndZeroSavingsRate()
    {
        // Act
        var report = await _service.GetReportAsync(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));

        // Assert
        Assert.Equal(0m, report.TotalIncome);
        Assert.Equal(0m, report.TotalExpenses);
        Assert.Equal(0m, report.NetSavings);
        Assert.Equal(0m, report.SavingsRate);
        Assert.Empty(report.ExpensesByCategory);
        Assert.Empty(report.IncomeByCategory);
        Assert.Empty(report.MonthlyTrends);
    }

    [Fact]
    public async Task GetReportAsync_WithExactDateBoundaries_ShouldIncludeBoundaryTransactions()
    {
        // Arrange
        var cat = new Category("Groceries", TransactionType.Expense);
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        var start = new DateTime(2026, 6, 1);
        var end = new DateTime(2026, 6, 30);

        _context.Transactions.AddRange(
            new Transaction(1000m, "Start boundary", start, TransactionType.Expense, cat.Id),
            new Transaction(2000m, "End boundary", end, TransactionType.Expense, cat.Id),
            new Transaction(5000m, "Before range", start.AddDays(-1), TransactionType.Expense, cat.Id),
            new Transaction(5000m, "After range", end.AddDays(1), TransactionType.Expense, cat.Id)
        );
        await _context.SaveChangesAsync();

        // Act
        var report = await _service.GetReportAsync(start, end);

        // Assert
        Assert.Equal(3000m, report.TotalExpenses);
    }

    [Fact]
    public async Task GetMonthlyTrendsAsync_ShouldReturnRequestedNumberOfMonths()
    {
        // Arrange
        var cat = new Category("Misc", TransactionType.Expense);
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        _context.Transactions.Add(
            new Transaction(25000m, "Item", new DateTime(2026, 11, 2), TransactionType.Expense, cat.Id)
        );
        await _context.SaveChangesAsync();

        // Act
        var trends = await _service.GetMonthlyTrendsAsync(6);

        // Assert
        Assert.Equal(6, trends.Count);
        var currentMonthTrend = trends.Last();
        Assert.Equal(2026, currentMonthTrend.Year);
        Assert.Equal(11, currentMonthTrend.Month);
        Assert.Equal(25000m, currentMonthTrend.Expenses);
        Assert.Equal(-25000m, currentMonthTrend.NetSavings);
    }
}
