using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Application.Services;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.UnitTests.Common;
using Xunit;

namespace FinanceTrack.UnitTests.Services;

public class CategoryServiceTests : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _service = new CategoryService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateCategory()
    {
        // Arrange
        var dto = new CreateCategoryDto
        {
            Name = "Healthcare",
            Type = TransactionType.Expense,
            Color = "#e91e63",
            Icon = "bi-heart-pulse"
        };

        // Act
        var created = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Healthcare", created.Name);
        Assert.Equal(TransactionType.Expense, created.Type);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateNameAndType_ShouldThrowApplicationValidationException()
    {
        // Arrange
        var category = new Category("Entertainment", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var duplicateDto = new CreateCategoryDto
        {
            Name = "Entertainment",
            Type = TransactionType.Expense
        };

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.CreateAsync(duplicateDto));
    }

    [Fact]
    public async Task UpdateAsync_WithValidDto_ShouldUpdateCategory()
    {
        // Arrange
        var category = new Category("Transport", TransactionType.Expense, "#111", "bi-car");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCategoryDto
        {
            Id = category.Id,
            Name = "Public Transit",
            Type = TransactionType.Expense,
            Color = "#222",
            Icon = "bi-bus"
        };

        // Act
        var updated = await _service.UpdateAsync(updateDto);

        // Assert
        Assert.Equal("Public Transit", updated.Name);
        Assert.Equal("#222", updated.Color);
        Assert.Equal("bi-bus", updated.Icon);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var updateDto = new UpdateCategoryDto
        {
            Id = 9999,
            Name = "Ghost",
            Type = TransactionType.Expense
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(updateDto));
    }

    [Fact]
    public async Task UpdateAsync_WhenChangingTypeWithExistingTransactions_ShouldThrowApplicationValidationException()
    {
        // Arrange
        var category = new Category("Freelance", TransactionType.Income);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var transaction = new Transaction(10000m, "Job 1", DateTime.UtcNow, TransactionType.Income, category.Id);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCategoryDto
        {
            Id = category.Id,
            Name = "Freelance",
            Type = TransactionType.Expense // Incompatible switch
        };

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.UpdateAsync(updateDto));
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryInUseByTransactions_ShouldThrowApplicationValidationException()
    {
        // Arrange
        var category = new Category("Groceries", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var transaction = new Transaction(3000m, "Store", DateTime.UtcNow, TransactionType.Expense, category.Id);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act & Assert
        Assert.False(await _service.CanDeleteAsync(category.Id));
        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.DeleteAsync(category.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryInUseByBudgetsOnly_ShouldThrowApplicationValidationException()
    {
        // Arrange: Category is only referenced by Budget, zero transactions
        var category = new Category("Clothing", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget = new Budget(30000m, 11, 2026, category.Id);
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        // Act & Assert
        Assert.False(await _service.CanDeleteAsync(category.Id));
        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.DeleteAsync(category.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenUnused_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var category = new Category("Old Category", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _service.DeleteAsync(category.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(await _service.GetByIdAsync(category.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentCategory_ShouldReturnFalse()
    {
        // Act
        var deleted = await _service.DeleteAsync(999);

        // Assert
        Assert.False(deleted);
    }
}
