using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Domain.Exceptions;
using Xunit;

namespace FinanceTrack.UnitTests.Domain;

public class BudgetTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldCreateBudget()
    {
        // Act
        var budget = new Budget(100000m, 11, 2026, 1);

        // Assert
        Assert.Equal(100000m, budget.Amount);
        Assert.Equal(11, budget.Month);
        Assert.Equal(2026, budget.Year);
        Assert.Equal(1, budget.CategoryId);
        Assert.Null(budget.Category);
        Assert.Null(budget.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithCategoryEntity_ShouldCreateBudgetAndAssignCategory()
    {
        // Arrange
        var category = new Category("Housing", TransactionType.Expense);
        category.Id = 15;

        // Act
        var budget = new Budget(250000m, 12, 2026, category);

        // Assert
        Assert.Equal(250000m, budget.Amount);
        Assert.Equal(12, budget.Month);
        Assert.Equal(2026, budget.Year);
        Assert.Equal(15, budget.CategoryId);
        Assert.Equal(category, budget.Category);
    }

    [Fact]
    public void Constructor_WithIncomeCategoryEntity_ShouldThrowDomainValidationException()
    {
        // Arrange: Budgets are not allowed on income categories
        var incomeCategory = new Category("Salary", TransactionType.Income);
        incomeCategory.Id = 1;

        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Budget(250000m, 12, 2026, incomeCategory));

        Assert.Equal(nameof(Budget.Category), ex.PropertyName);
    }

    [Fact]
    public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Budget(50000m, 1, 2026, (Category)null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    [InlineData(-0.01)]
    public void Constructor_WithInvalidAmount_ShouldThrowDomainValidationException(decimal invalidAmount)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Budget(invalidAmount, 11, 2026, 1));

        Assert.Equal(nameof(Budget.Amount), ex.PropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    [InlineData(100)]
    public void Constructor_WithInvalidMonth_ShouldThrowDomainValidationException(int invalidMonth)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Budget(50000m, invalidMonth, 2026, 1));

        Assert.Equal(nameof(Budget.Month), ex.PropertyName);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(1900)]
    [InlineData(2101)]
    public void Constructor_WithInvalidYear_ShouldThrowDomainValidationException(int invalidYear)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Budget(50000m, 6, invalidYear, 1));

        Assert.Equal(nameof(Budget.Year), ex.PropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidCategoryId_ShouldThrowDomainValidationException(int invalidCategoryId)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Budget(50000m, 6, 2026, invalidCategoryId));

        Assert.Equal(nameof(Budget.CategoryId), ex.PropertyName);
    }

    [Fact]
    public void Update_WithValidArguments_ShouldUpdateFieldsAndMarkUpdated()
    {
        // Arrange
        var budget = new Budget(50000m, 5, 2026, 1);

        // Act
        budget.Update(65000m, 6, 2026, 2);

        // Assert
        Assert.Equal(65000m, budget.Amount);
        Assert.Equal(6, budget.Month);
        Assert.Equal(2026, budget.Year);
        Assert.Equal(2, budget.CategoryId);
        Assert.NotNull(budget.UpdatedAt);
    }

    [Fact]
    public void AssignCategory_WithExpenseCategory_ShouldSetCategoryAndId()
    {
        // Arrange
        var budget = new Budget(50000m, 5, 2026, 1);
        var category = new Category("Entertainment", TransactionType.Expense);
        category.Id = 4;

        // Act
        budget.AssignCategory(category);

        // Assert
        Assert.Equal(category, budget.Category);
        Assert.Equal(4, budget.CategoryId);
        Assert.NotNull(budget.UpdatedAt);
    }

    [Fact]
    public void AssignCategory_WithIncomeCategory_ShouldThrowDomainValidationException()
    {
        // Arrange: Income category assigned to budget
        var budget = new Budget(50000m, 5, 2026, 1);
        var incomeCategory = new Category("Salary", TransactionType.Income);
        incomeCategory.Id = 7;

        // Act & Assert
        var ex = Assert.Throws<DomainValidationException>(() =>
            budget.AssignCategory(incomeCategory));

        Assert.Equal(nameof(Budget.Category), ex.PropertyName);
    }

    [Fact]
    public void AssignCategory_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var budget = new Budget(50000m, 5, 2026, 1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => budget.AssignCategory(null!));
    }

    [Fact]
    public void HasConflictWith_ShouldCorrectlyIdentifyMatchingCategoryMonthAndYear()
    {
        // Arrange
        var budget = new Budget(75000m, 8, 2026, 5);

        // Act & Assert
        Assert.True(budget.HasConflictWith(5, 8, 2026));
        Assert.False(budget.HasConflictWith(6, 8, 2026)); // different category
        Assert.False(budget.HasConflictWith(5, 9, 2026)); // different month
        Assert.False(budget.HasConflictWith(5, 8, 2025)); // different year
    }
}
