using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Domain.Exceptions;
using Xunit;

namespace FinanceTrack.UnitTests.Domain;

public class CategoryTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldCreateCategory()
    {
        // Act
        var category = new Category("Groceries", TransactionType.Expense, "#ff5722", "bi-cart");

        // Assert
        Assert.Equal("Groceries", category.Name);
        Assert.Equal(TransactionType.Expense, category.Type);
        Assert.Equal("#ff5722", category.Color);
        Assert.Equal("bi-cart", category.Icon);
        Assert.Empty(category.Transactions);
        Assert.Empty(category.Budgets);
        Assert.Null(category.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithDefaultColorAndIcon_ShouldUseFallbacks()
    {
        // Act
        var category = new Category("Salary", TransactionType.Income, "", "");

        // Assert
        Assert.Equal("Salary", category.Name);
        Assert.Equal("#0d6efd", category.Color);
        Assert.Equal("bi-tag", category.Icon);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ShouldThrowDomainValidationException(string? invalidName)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Category(invalidName!, TransactionType.Expense));

        Assert.Equal(nameof(Category.Name), ex.PropertyName);
    }

    [Fact]
    public void Constructor_WithNameExceeding50Chars_ShouldThrowDomainValidationException()
    {
        // Arrange
        var longName = new string('A', 51);

        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Category(longName, TransactionType.Expense));

        Assert.Equal(nameof(Category.Name), ex.PropertyName);
    }

    [Fact]
    public void Constructor_WithInvalidTransactionType_ShouldThrowDomainValidationException()
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Category("Health", (TransactionType)99));

        Assert.Equal(nameof(Category.Type), ex.PropertyName);
    }

    [Fact]
    public void Update_WithValidArguments_ShouldUpdateFieldsAndMarkUpdated()
    {
        // Arrange
        var category = new Category("Transport", TransactionType.Expense, "#123456", "bi-bus");

        // Act
        category.Update("Public Transit", TransactionType.Expense, "#654321", "bi-train");

        // Assert
        Assert.Equal("Public Transit", category.Name);
        Assert.Equal("#654321", category.Color);
        Assert.Equal("bi-train", category.Icon);
        Assert.NotNull(category.UpdatedAt);
    }

    [Fact]
    public void AddTransaction_WithMatchingType_ShouldAddTransactionAndLinkCategory()
    {
        // Arrange
        var category = new Category("Groceries", TransactionType.Expense);
        category.Id = 1;
        var transaction = new Transaction(4500m, "Bread & Milk", DateTime.UtcNow, TransactionType.Expense, 1);

        // Act
        category.AddTransaction(transaction);

        // Assert
        Assert.Single(category.Transactions);
        Assert.Contains(transaction, category.Transactions);
        Assert.Equal(category, transaction.Category);
    }

    [Fact]
    public void AddTransaction_WithIncompatibleType_ShouldThrowDomainValidationException()
    {
        // Arrange: Category is Expense, Transaction is Income
        var category = new Category("Groceries", TransactionType.Expense);
        var incomeTransaction = new Transaction(50000m, "Salary", DateTime.UtcNow, TransactionType.Income, 1);

        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            category.AddTransaction(incomeTransaction));

        Assert.Equal(nameof(Transaction.Type), ex.PropertyName);
    }

    [Fact]
    public void AddTransaction_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var category = new Category("Groceries", TransactionType.Expense);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => category.AddTransaction(null!));
    }

    [Fact]
    public void AddBudget_WithExpenseCategory_ShouldAddBudgetSuccessfully()
    {
        // Arrange
        var category = new Category("Groceries", TransactionType.Expense);
        category.Id = 1;
        var budget = new Budget(60000m, 11, 2026, 1);

        // Act
        category.AddBudget(budget);

        // Assert
        Assert.Single(category.Budgets);
        Assert.Contains(budget, category.Budgets);
        Assert.Equal(category, budget.Category);
    }

    [Fact]
    public void AddBudget_WithIncomeCategory_ShouldThrowDomainValidationException()
    {
        // Arrange: Category is Income
        var category = new Category("Dividends", TransactionType.Income);
        var budget = new Budget(60000m, 11, 2026, 1);

        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            category.AddBudget(budget));

        Assert.Equal(nameof(Category.Type), ex.PropertyName);
    }

    [Fact]
    public void AddBudget_WithDuplicateMonthAndYear_ShouldThrowDomainValidationException()
    {
        // Arrange
        var category = new Category("Dining", TransactionType.Expense);
        category.Id = 2;
        var budget1 = new Budget(30000m, 11, 2026, 2);
        var budget2 = new Budget(40000m, 11, 2026, 2);

        category.AddBudget(budget1);

        // Act & Assert: Attempting to add duplicate budget for 11/2026
        var ex = Assert.Throws<DomainValidationException>(() =>
            category.AddBudget(budget2));

        Assert.Equal(nameof(Category.Budgets), ex.PropertyName);
    }

    [Fact]
    public void AddBudget_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var category = new Category("Utilities", TransactionType.Expense);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => category.AddBudget(null!));
    }

    [Fact]
    public void HasBudgetFor_ShouldReturnTrueIfBudgetExistsForPeriod()
    {
        // Arrange
        var category = new Category("Transport", TransactionType.Expense);
        category.Id = 3;
        category.AddBudget(new Budget(20000m, 5, 2026, 3));

        // Act & Assert
        Assert.True(category.HasBudgetFor(5, 2026));
        Assert.False(category.HasBudgetFor(6, 2026));
        Assert.False(category.HasBudgetFor(5, 2025));
    }
}
