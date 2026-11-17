using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Application.Services;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.UnitTests.Common;
using Xunit;

namespace FinanceTrack.UnitTests.Services;

public class TransactionServiceTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _service = new TransactionService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateTransaction()
    {
        // Arrange
        var category = new Category("Groceries", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var dto = new CreateTransactionDto
        {
            Amount = 12000m,
            Description = "Weekly Groceries",
            Date = new DateTime(2026, 11, 15),
            Type = TransactionType.Expense,
            CategoryId = category.Id
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(12000m, result.Amount);
        Assert.Equal("Weekly Groceries", result.Description);
        Assert.Equal("Groceries", result.CategoryName);
    }

    [Fact]
    public async Task CreateAsync_WithMissingCategory_ShouldThrowNotFoundException()
    {
        // Arrange
        var dto = new CreateTransactionDto
        {
            Amount = 5000m,
            Description = "Coffee",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense,
            CategoryId = 999
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithIncompatibleCategoryType_ShouldThrowApplicationValidationException()
    {
        // Arrange: Category is Income, Transaction DTO is Expense
        var incomeCat = new Category("Salary", TransactionType.Income);
        _context.Categories.Add(incomeCat);
        await _context.SaveChangesAsync();

        var dto = new CreateTransactionDto
        {
            Amount = 5000m,
            Description = "Dinner",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense,
            CategoryId = incomeCat.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_WithValidDto_ShouldUpdateTransaction()
    {
        // Arrange
        var category1 = new Category("Groceries", TransactionType.Expense);
        var category2 = new Category("Dining Out", TransactionType.Expense);
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        var transaction = new Transaction(5000m, "Lunch", new DateTime(2026, 11, 10), TransactionType.Expense, category1.Id);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateTransactionDto
        {
            Id = transaction.Id,
            Amount = 7500m,
            Description = "Fancy Lunch",
            Date = new DateTime(2026, 11, 12),
            Type = TransactionType.Expense,
            CategoryId = category2.Id
        };

        // Act
        var updated = await _service.UpdateAsync(updateDto);

        // Assert
        Assert.Equal(7500m, updated.Amount);
        Assert.Equal("Fancy Lunch", updated.Description);
        Assert.Equal(category2.Id, updated.CategoryId);
        Assert.Equal("Dining Out", updated.CategoryName);
    }

    [Fact]
    public async Task UpdateAsync_WithMissingTransaction_ShouldThrowNotFoundException()
    {
        // Arrange
        var updateDto = new UpdateTransactionDto
        {
            Id = 999,
            Amount = 1000m,
            Description = "None",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense,
            CategoryId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(updateDto));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingTransaction_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var cat = new Category("Fuel", TransactionType.Expense);
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        var transaction = new Transaction(10000m, "Gas station", DateTime.UtcNow, TransactionType.Expense, cat.Id);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _service.DeleteAsync(transaction.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(await _service.GetByIdAsync(transaction.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentTransaction_ShouldReturnFalse()
    {
        // Act
        var deleted = await _service.DeleteAsync(9999);

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task GetPagedAsync_WithFiltersAndPagination_ShouldFilterAndPaginateCorrectly()
    {
        // Arrange
        var expenseCat = new Category("Transport", TransactionType.Expense);
        var incomeCat = new Category("Freelance", TransactionType.Income);
        _context.Categories.AddRange(expenseCat, incomeCat);
        await _context.SaveChangesAsync();

        _context.Transactions.AddRange(
            new Transaction(1000m, "Taxi to airport", new DateTime(2026, 11, 1), TransactionType.Expense, expenseCat.Id),
            new Transaction(500m, "Bus ticket", new DateTime(2026, 11, 5), TransactionType.Expense, expenseCat.Id),
            new Transaction(2000m, "Metro pass", new DateTime(2026, 11, 10), TransactionType.Expense, expenseCat.Id),
            new Transaction(50000m, "Project Alpha payment", new DateTime(2026, 11, 12), TransactionType.Income, incomeCat.Id)
        );
        await _context.SaveChangesAsync();

        // Filter 1: Search by text
        var searchResult = await _service.GetPagedAsync(new TransactionFilterDto { SearchTerm = "airport" });
        Assert.Single(searchResult.Items);
        Assert.Equal("Taxi to airport", searchResult.Items[0].Description);

        // Filter 2: Filter by Type
        var expenseResult = await _service.GetPagedAsync(new TransactionFilterDto { Type = TransactionType.Expense });
        Assert.Equal(3, expenseResult.TotalCount);

        // Filter 3: Filter by Date Range
        var dateRangeResult = await _service.GetPagedAsync(new TransactionFilterDto
        {
            StartDate = new DateTime(2026, 11, 4),
            EndDate = new DateTime(2026, 11, 11)
        });
        Assert.Equal(2, dateRangeResult.TotalCount);

        // Filter 4: Sorting by amount descending
        var sortedResult = await _service.GetPagedAsync(new TransactionFilterDto { SortBy = "amount_desc", PageSize = 2 });
        Assert.Equal(2, sortedResult.Items.Count);
        Assert.Equal(50000m, sortedResult.Items[0].Amount);
        Assert.Equal(2000m, sortedResult.Items[1].Amount);
        Assert.Equal(4, sortedResult.TotalCount);
        Assert.Equal(2, sortedResult.TotalPages);
    }
}
