using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Domain.Exceptions;
using Xunit;

namespace FinanceTrack.UnitTests.Domain;

public class TransactionTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldCreateTransaction()
    {
        // Arrange
        var date = new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var transaction = new Transaction(15000.50m, "Grocery Shopping", date, TransactionType.Expense, 1);

        // Assert
        Assert.Equal(15000.50m, transaction.Amount);
        Assert.Equal("Grocery Shopping", transaction.Description);
        Assert.Equal(date, transaction.Date);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(1, transaction.CategoryId);
        Assert.True(transaction.CreatedAt <= DateTime.UtcNow);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithCategoryEntity_ShouldCreateTransactionAndSetType()
    {
        // Arrange
        var category = new Category("Groceries", TransactionType.Expense);
        category.Id = 2;
        var date = DateTime.UtcNow;

        // Act
        var transaction = new Transaction(2500m, "Supermarket", date, category);

        // Assert
        Assert.Equal(2500m, transaction.Amount);
        Assert.Equal("Supermarket", transaction.Description);
        Assert.Equal(date, transaction.Date);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(2, transaction.CategoryId);
        Assert.Equal(category, transaction.Category);
    }

    [Fact]
    public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Transaction(2500m, "Supermarket", DateTime.UtcNow, (Category)null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void Constructor_WithInvalidAmount_ShouldThrowDomainValidationException(decimal invalidAmount)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Transaction(invalidAmount, "Salary", DateTime.UtcNow, TransactionType.Income, 1));

        Assert.Equal(nameof(Transaction.Amount), ex.PropertyName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyDescription_ShouldThrowDomainValidationException(string? invalidDescription)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Transaction(1000m, invalidDescription!, DateTime.UtcNow, TransactionType.Expense, 1));

        Assert.Equal(nameof(Transaction.Description), ex.PropertyName);
    }

    [Fact]
    public void Constructor_WithDescriptionExceedingMaxLength_ShouldThrowDomainValidationException()
    {
        // Arrange
        var longDescription = new string('A', 201);

        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Transaction(1000m, longDescription, DateTime.UtcNow, TransactionType.Expense, 1));

        Assert.Equal(nameof(Transaction.Description), ex.PropertyName);
    }

    [Fact]
    public void Constructor_WithDefaultDate_ShouldThrowDomainValidationException()
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Transaction(1000m, "Payment", default, TransactionType.Expense, 1));

        Assert.Equal(nameof(Transaction.Date), ex.PropertyName);
    }

    [Fact]
    public void Constructor_WithInvalidTransactionType_ShouldThrowDomainValidationException()
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Transaction(1000m, "Payment", DateTime.UtcNow, (TransactionType)999, 1));

        Assert.Equal(nameof(Transaction.Type), ex.PropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidCategoryId_ShouldThrowDomainValidationException(int invalidCategoryId)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Transaction(1000m, "Payment", DateTime.UtcNow, TransactionType.Expense, invalidCategoryId));

        Assert.Equal(nameof(Transaction.CategoryId), ex.PropertyName);
    }

    [Fact]
    public void Update_WithValidArguments_ShouldUpdateFieldsAndMarkUpdated()
    {
        // Arrange
        var transaction = new Transaction(5000m, "Lunch", DateTime.UtcNow, TransactionType.Expense, 1);
        var newDate = DateTime.UtcNow.AddDays(1);

        // Act
        transaction.Update(7500m, "Business Dinner", newDate, TransactionType.Expense, 2);

        // Assert
        Assert.Equal(7500m, transaction.Amount);
        Assert.Equal("Business Dinner", transaction.Description);
        Assert.Equal(newDate, transaction.Date);
        Assert.Equal(2, transaction.CategoryId);
        Assert.NotNull(transaction.UpdatedAt);
    }

    [Fact]
    public void Update_WithIncompatibleCategoryType_ShouldThrowDomainValidationException()
    {
        // Arrange
        var category = new Category("Salary", TransactionType.Income);
        category.Id = 10;
        var transaction = new Transaction(5000m, "Paycheck", DateTime.UtcNow, category);

        // Act & Assert: Attempting to switch type to Expense while Category is still Salary (Income)
        var ex = Assert.Throws<DomainValidationException>(() =>
            transaction.Update(5000m, "Paycheck", DateTime.UtcNow, TransactionType.Expense, 10));

        Assert.Equal(nameof(Transaction.Type), ex.PropertyName);
    }

    [Fact]
    public void AssignCategory_WithCompatibleCategoryType_ShouldUpdateCategoryAndId()
    {
        // Arrange
        var transaction = new Transaction(5000m, "Groceries", DateTime.UtcNow, TransactionType.Expense, 1);
        var category = new Category("Dining Out", TransactionType.Expense, "#ff5722", "bi-cup");
        category.Id = 5;

        // Act
        transaction.AssignCategory(category);

        // Assert
        Assert.Equal(5, transaction.CategoryId);
        Assert.Equal(category, transaction.Category);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.NotNull(transaction.UpdatedAt);
    }

    [Fact]
    public void AssignCategory_WithIncompatibleCategoryType_ShouldThrowDomainValidationException()
    {
        // Arrange: Transaction is Expense, but Category is Income
        var transaction = new Transaction(5000m, "Freelance", DateTime.UtcNow, TransactionType.Expense, 1);
        var incomeCategory = new Category("Salary", TransactionType.Income);
        incomeCategory.Id = 3;

        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            transaction.AssignCategory(incomeCategory));

        Assert.Equal(nameof(Transaction.Type), ex.PropertyName);
    }

    [Fact]
    public void AssignCategory_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var transaction = new Transaction(5000m, "Freelance", DateTime.UtcNow, TransactionType.Expense, 1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => transaction.AssignCategory(null!));
    }
}
