using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Application.Services;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.UnitTests.Common;
using Xunit;

namespace FinanceTrack.UnitTests.Services;

public class BudgetServiceTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _dateTimeProvider = new TestDateTimeProvider();
        _service = new BudgetService(_context, _dateTimeProvider);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateBudget()
    {
        // Arrange
        var category = new Category("Dining", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var dto = new CreateBudgetDto
        {
            Amount = 50000m,
            Month = 11,
            Year = 2026,
            CategoryId = category.Id
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(50000m, result.Amount);
        Assert.Equal(11, result.Month);
        Assert.Equal(2026, result.Year);
        Assert.Equal("Dining", result.CategoryName);
    }

    [Fact]
    public async Task CreateAsync_OnIncomeCategory_ShouldThrowApplicationValidationException()
    {
        // Arrange: Income category cannot have budget
        var incomeCategory = new Category("Dividends", TransactionType.Income);
        _context.Categories.Add(incomeCategory);
        await _context.SaveChangesAsync();

        var dto = new CreateBudgetDto
        {
            Amount = 50000m,
            Month = 11,
            Year = 2026,
            CategoryId = incomeCategory.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCategoryMonthAndYear_ShouldThrowApplicationValidationException()
    {
        // Arrange
        var category = new Category("Groceries", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget = new Budget(60000m, 11, 2026, category.Id);
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        var duplicateDto = new CreateBudgetDto
        {
            Amount = 70000m,
            Month = 11,
            Year = 2026,
            CategoryId = category.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.CreateAsync(duplicateDto));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateCategoryMonthAndYear_ShouldThrowApplicationValidationException()
    {
        // Arrange
        var category = new Category("Groceries", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget1 = new Budget(60000m, 10, 2026, category.Id);
        var budget2 = new Budget(70000m, 11, 2026, category.Id);
        _context.Budgets.AddRange(budget1, budget2);
        await _context.SaveChangesAsync();

        var duplicateUpdate = new UpdateBudgetDto
        {
            Id = budget2.Id,
            Amount = 80000m,
            Month = 10, // Clashes with budget1
            Year = 2026,
            CategoryId = category.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.UpdateAsync(duplicateUpdate));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var updateDto = new UpdateBudgetDto
        {
            Id = 9999,
            Amount = 50000m,
            Month = 1,
            Year = 2026,
            CategoryId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(updateDto));
    }

    [Fact]
    public async Task BudgetCalculations_ShouldCalculateSpentRemainingAndPercentageCorrectly()
    {
        // Arrange
        var category = new Category("Shopping", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget = new Budget(100000m, 11, 2026, category.Id);
        _context.Budgets.Add(budget);

        // Add matching expense transactions in same month & year
        _context.Transactions.AddRange(
            new Transaction(30000m, "Mall", new DateTime(2026, 11, 5), TransactionType.Expense, category.Id),
            new Transaction(45000m, "Electronics", new DateTime(2026, 11, 15), TransactionType.Expense, category.Id),
            // Unrelated transaction (different month)
            new Transaction(20000m, "Previous Month", new DateTime(2026, 10, 20), TransactionType.Expense, category.Id)
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(budget.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100000m, result.Amount);
        Assert.Equal(75000m, result.AmountSpent);
        Assert.Equal(25000m, result.RemainingAmount);
        Assert.Equal(75.0m, result.PercentageUsed);
        Assert.False(result.IsOverBudget);
    }

    [Fact]
    public async Task BudgetCalculations_WhenOverBudget_ShouldFlagIsOverBudget()
    {
        // Arrange
        var category = new Category("Gaming", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget = new Budget(20000m, 11, 2026, category.Id);
        _context.Budgets.Add(budget);

        _context.Transactions.Add(
            new Transaction(25000m, "Console", new DateTime(2026, 11, 1), TransactionType.Expense, category.Id)
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(budget.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25000m, result.AmountSpent);
        Assert.Equal(-5000m, result.RemainingAmount);
        Assert.Equal(125.0m, result.PercentageUsed);
        Assert.True(result.IsOverBudget);
    }

    [Fact]
    public async Task GetAllAsync_WhenZeroTransactionsSpent_ShouldReturnZeroSpentAndFullRemaining()
    {
        // Arrange
        var category = new Category("Savings", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget = new Budget(50000m, 11, 2026, category.Id);
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        // Act
        var all = await _service.GetAllAsync(2026, 11);

        // Assert
        Assert.Single(all);
        Assert.Equal(0m, all[0].AmountSpent);
        Assert.Equal(50000m, all[0].RemainingAmount);
        Assert.Equal(0m, all[0].PercentageUsed);
        Assert.False(all[0].IsOverBudget);
    }

    [Fact]
    public async Task GetByPeriodAsync_WhenBudgetDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByPeriodAsync(1, 1, 2026);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingBudget_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var category = new Category("Travel", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget = new Budget(50000m, 11, 2026, category.Id);
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _service.DeleteAsync(budget.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(await _service.GetByIdAsync(budget.Id));
    }
}
