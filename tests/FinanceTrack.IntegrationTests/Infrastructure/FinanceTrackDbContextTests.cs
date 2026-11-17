using System.Globalization;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceTrack.IntegrationTests.Infrastructure;

public class FinanceTrackDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly FinanceTrackDbContext _context;

    public FinanceTrackDbContextTests()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FinanceTrackDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new FinanceTrackDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    // ========================================================================
    // DATABASE CREATION TESTS
    // ========================================================================

    [Fact]
    public async Task Database_CanBeCreatedSuccessfully()
    {
        var canConnect = await _context.Database.CanConnectAsync();
        Assert.True(canConnect);
    }

    [Fact]
    public void Database_HasTransactionsTable()
    {
        Assert.NotNull(_context.Transactions);
    }

    [Fact]
    public void Database_HasCategoriesTable()
    {
        Assert.NotNull(_context.Categories);
    }

    [Fact]
    public void Database_HasBudgetsTable()
    {
        Assert.NotNull(_context.Budgets);
    }

    // ========================================================================
    // SEED DATA VERIFICATION TESTS
    // ========================================================================

    [Fact]
    public async Task SeedData_Contains13DefaultCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        Assert.Equal(13, categories.Count);
    }

    [Fact]
    public async Task SeedData_Contains8ExpenseCategories()
    {
        var expenseCategories = await _context.Categories
            .Where(c => c.Type == TransactionType.Expense)
            .ToListAsync();

        Assert.Equal(8, expenseCategories.Count);
    }

    [Fact]
    public async Task SeedData_Contains5IncomeCategories()
    {
        var incomeCategories = await _context.Categories
            .Where(c => c.Type == TransactionType.Income)
            .ToListAsync();

        Assert.Equal(5, incomeCategories.Count);
    }

    [Theory]
    [InlineData("Food", TransactionType.Expense)]
    [InlineData("Transport", TransactionType.Expense)]
    [InlineData("Shopping", TransactionType.Expense)]
    [InlineData("Housing", TransactionType.Expense)]
    [InlineData("Entertainment", TransactionType.Expense)]
    [InlineData("Health", TransactionType.Expense)]
    [InlineData("Education", TransactionType.Expense)]
    [InlineData("Other", TransactionType.Expense)]
    [InlineData("Salary", TransactionType.Income)]
    [InlineData("Freelance", TransactionType.Income)]
    [InlineData("Business", TransactionType.Income)]
    [InlineData("Investments", TransactionType.Income)]
    [InlineData("Other Income", TransactionType.Income)]
    public async Task SeedData_ContainsExpectedCategory(string name, TransactionType type)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == name && c.Type == type);

        Assert.NotNull(category);
        Assert.False(string.IsNullOrEmpty(category.Color));
        Assert.False(string.IsNullOrEmpty(category.Icon));
    }

    // ========================================================================
    // TRANSACTION CRUD TESTS
    // ========================================================================

    [Fact]
    public async Task Transaction_CanBeCreatedAndRetrieved()
    {
        var transaction = new Transaction(
            1500.00m, "Test salary", DateTime.UtcNow,
            TransactionType.Income, 9); // Salary category

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var saved = await _context.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(saved);
        Assert.Equal(1500.00m, saved.Amount);
        Assert.Equal("Test salary", saved.Description);
        Assert.Equal(TransactionType.Income, saved.Type);
        Assert.Equal(9, saved.CategoryId);
    }

    [Fact]
    public async Task Transaction_CanBeUpdated()
    {
        var transaction = new Transaction(
            500.00m, "Groceries", DateTime.UtcNow,
            TransactionType.Expense, 1);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        transaction.Update(750.00m, "Updated groceries", DateTime.UtcNow,
            TransactionType.Expense, 1);
        await _context.SaveChangesAsync();

        var saved = await _context.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(saved);
        Assert.Equal(750.00m, saved.Amount);
        Assert.Equal("Updated groceries", saved.Description);
    }

    [Fact]
    public async Task Transaction_CanBeDeleted()
    {
        var transaction = new Transaction(
            300.00m, "Delete me", DateTime.UtcNow,
            TransactionType.Expense, 1);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        var id = transaction.Id;

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        var saved = await _context.Transactions.FindAsync(id);
        Assert.Null(saved);
    }

    [Fact]
    public async Task Transaction_AmountHasCorrectPrecision()
    {
        var transaction = new Transaction(
            12345.67m, "Precision test", DateTime.UtcNow,
            TransactionType.Expense, 1);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _context.Entry(transaction).State = EntityState.Detached;
        var saved = await _context.Transactions.FindAsync(transaction.Id);

        Assert.NotNull(saved);
        Assert.Equal(12345.67m, saved.Amount);
    }

    // ========================================================================
    // CATEGORY CRUD TESTS
    // ========================================================================

    [Fact]
    public async Task Category_CanBeCreatedAndRetrieved()
    {
        var category = new Category("Custom Expense", TransactionType.Expense, "#ff0000", "bi-star");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var saved = await _context.Categories.FindAsync(category.Id);
        Assert.NotNull(saved);
        Assert.Equal("Custom Expense", saved.Name);
        Assert.Equal("#ff0000", saved.Color);
        Assert.Equal("bi-star", saved.Icon);
        Assert.Equal(TransactionType.Expense, saved.Type);
    }

    [Fact]
    public async Task Category_CanBeUpdated()
    {
        var category = new Category("Original", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        category.Update("Renamed", TransactionType.Expense, "#00ff00", "bi-pencil");
        await _context.SaveChangesAsync();

        var saved = await _context.Categories.FindAsync(category.Id);
        Assert.NotNull(saved);
        Assert.Equal("Renamed", saved.Name);
        Assert.Equal("#00ff00", saved.Color);
    }

    [Fact]
    public async Task Category_CanBeDeletedWhenUnreferenced()
    {
        var category = new Category("Deletable", TransactionType.Expense);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        var id = category.Id;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        var saved = await _context.Categories.FindAsync(id);
        Assert.Null(saved);
    }

    // ========================================================================
    // BUDGET CRUD TESTS
    // ========================================================================

    [Fact]
    public async Task Budget_CanBeCreatedAndRetrieved()
    {
        var budget = new Budget(50000m, 11, 2026, 1); // Food category
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        var saved = await _context.Budgets.FindAsync(budget.Id);
        Assert.NotNull(saved);
        Assert.Equal(50000m, saved.Amount);
        Assert.Equal(11, saved.Month);
        Assert.Equal(2026, saved.Year);
        Assert.Equal(1, saved.CategoryId);
    }

    [Fact]
    public async Task Budget_CanBeUpdated()
    {
        var budget = new Budget(30000m, 10, 2026, 2);
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        budget.Update(45000m, 10, 2026, 2);
        await _context.SaveChangesAsync();

        var saved = await _context.Budgets.FindAsync(budget.Id);
        Assert.NotNull(saved);
        Assert.Equal(45000m, saved.Amount);
    }

    [Fact]
    public async Task Budget_CanBeDeleted()
    {
        var budget = new Budget(20000m, 12, 2026, 3);
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        var id = budget.Id;

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();

        var saved = await _context.Budgets.FindAsync(id);
        Assert.Null(saved);
    }

    [Fact]
    public async Task Budget_AmountHasCorrectPrecision()
    {
        var budget = new Budget(99999.99m, 1, 2026, 1);
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        _context.Entry(budget).State = EntityState.Detached;
        var saved = await _context.Budgets.FindAsync(budget.Id);

        Assert.NotNull(saved);
        Assert.Equal(99999.99m, saved.Amount);
    }

    // ========================================================================
    // FOREIGN KEY CONSTRAINT TESTS
    // ========================================================================

    [Fact]
    public async Task ForeignKey_CannotDeleteCategoryWithTransactions()
    {
        // Category 1 (Food) is seeded — add a transaction referencing it
        var transaction = new Transaction(
            100m, "FK test", DateTime.UtcNow,
            TransactionType.Expense, 1);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var category = await _context.Categories.FindAsync(1);
        Assert.NotNull(category);

        _context.Categories.Remove(category);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task ForeignKey_CannotDeleteCategoryWithBudgets()
    {
        var budget = new Budget(10000m, 6, 2026, 2); // Transport category
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var category = await _context.Categories.FindAsync(2);
        Assert.NotNull(category);

        _context.Categories.Remove(category);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task ForeignKey_TransactionMustReferenceExistingCategory()
    {
        var transaction = new Transaction(
            100m, "Invalid FK", DateTime.UtcNow,
            TransactionType.Expense, 9999); // Non-existent category

        _context.Transactions.Add(transaction);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task ForeignKey_BudgetMustReferenceExistingCategory()
    {
        var budget = new Budget(5000m, 3, 2026, 9999); // Non-existent category
        _context.Budgets.Add(budget);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());
    }

    // ========================================================================
    // UNIQUE CONSTRAINT TESTS
    // ========================================================================

    [Fact]
    public async Task UniqueConstraint_CannotCreateDuplicateBudget()
    {
        var budget1 = new Budget(10000m, 11, 2026, 1);
        _context.Budgets.Add(budget1);
        await _context.SaveChangesAsync();

        var budget2 = new Budget(20000m, 11, 2026, 1); // Same category, month, year
        _context.Budgets.Add(budget2);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task UniqueConstraint_AllowsDifferentMonthSameCategoryAndYear()
    {
        var budget1 = new Budget(10000m, 1, 2026, 1);
        var budget2 = new Budget(15000m, 2, 2026, 1);

        _context.Budgets.AddRange(budget1, budget2);
        await _context.SaveChangesAsync();

        Assert.True(budget1.Id > 0);
        Assert.True(budget2.Id > 0);
        Assert.NotEqual(budget1.Id, budget2.Id);
    }

    [Fact]
    public async Task UniqueConstraint_AllowsDifferentYearSameCategoryAndMonth()
    {
        var budget1 = new Budget(10000m, 6, 2025, 1);
        var budget2 = new Budget(12000m, 6, 2026, 1);

        _context.Budgets.AddRange(budget1, budget2);
        await _context.SaveChangesAsync();

        Assert.True(budget1.Id > 0);
        Assert.True(budget2.Id > 0);
    }

    [Fact]
    public async Task UniqueConstraint_AllowsSameMonthYearDifferentCategory()
    {
        var budget1 = new Budget(10000m, 11, 2026, 1); // Food
        var budget2 = new Budget(5000m, 11, 2026, 2);  // Transport

        _context.Budgets.AddRange(budget1, budget2);
        await _context.SaveChangesAsync();

        Assert.True(budget1.Id > 0);
        Assert.True(budget2.Id > 0);
    }

    // ========================================================================
    // RELATIONSHIP LOADING TESTS
    // ========================================================================

    [Fact]
    public async Task Relationship_TransactionLoadsCategory()
    {
        var transaction = new Transaction(
            500m, "With category", DateTime.UtcNow,
            TransactionType.Expense, 1);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _context.Entry(transaction).State = EntityState.Detached;

        var loaded = await _context.Transactions
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == transaction.Id);

        Assert.NotNull(loaded.Category);
        Assert.Equal("Food", loaded.Category.Name);
    }

    [Fact]
    public async Task Relationship_CategoryLoadsTransactions()
    {
        // Add two transactions to Food category
        _context.Transactions.AddRange(
            new Transaction(100m, "Lunch", DateTime.UtcNow, TransactionType.Expense, 1),
            new Transaction(200m, "Dinner", DateTime.UtcNow, TransactionType.Expense, 1)
        );
        await _context.SaveChangesAsync();

        var category = await _context.Categories
            .Include(c => c.Transactions)
            .FirstAsync(c => c.Id == 1);

        Assert.Equal(2, category.Transactions.Count);
    }

    [Fact]
    public async Task Relationship_CategoryLoadsBudgets()
    {
        _context.Budgets.AddRange(
            new Budget(10000m, 10, 2026, 1),
            new Budget(12000m, 11, 2026, 1)
        );
        await _context.SaveChangesAsync();

        var category = await _context.Categories
            .Include(c => c.Budgets)
            .FirstAsync(c => c.Id == 1);

        Assert.Equal(2, category.Budgets.Count);
    }

    [Fact]
    public async Task Relationship_BudgetLoadsCategory()
    {
        var budget = new Budget(25000m, 12, 2026, 4); // Housing category
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        _context.Entry(budget).State = EntityState.Detached;

        var loaded = await _context.Budgets
            .Include(b => b.Category)
            .FirstAsync(b => b.Id == budget.Id);

        Assert.NotNull(loaded.Category);
        Assert.Equal("Housing", loaded.Category.Name);
    }

    // ========================================================================
    // QUERY AND INDEX TESTS
    // ========================================================================

    [Fact]
    public async Task Query_CanFilterTransactionsByDateRange()
    {
        var now = new DateTime(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);
        _context.Transactions.AddRange(
            new Transaction(100m, "Before", now.AddDays(-10), TransactionType.Expense, 1),
            new Transaction(200m, "During", now, TransactionType.Expense, 1),
            new Transaction(300m, "After", now.AddDays(10), TransactionType.Expense, 1)
        );
        await _context.SaveChangesAsync();

        var start = now.AddDays(-5);
        var end = now.AddDays(5);
        var filtered = await _context.Transactions
            .Where(t => t.Date >= start && t.Date <= end)
            .ToListAsync();

        Assert.Single(filtered);
        Assert.Equal("During", filtered[0].Description);
    }

    [Fact]
    public async Task Query_CanFilterTransactionsByType()
    {
        _context.Transactions.AddRange(
            new Transaction(100m, "Expense1", DateTime.UtcNow, TransactionType.Expense, 1),
            new Transaction(200m, "Income1", DateTime.UtcNow, TransactionType.Income, 9)
        );
        await _context.SaveChangesAsync();

        var expenses = await _context.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .ToListAsync();

        Assert.Single(expenses);
        Assert.Equal("Expense1", expenses[0].Description);
    }

    [Fact]
    public async Task Query_CanFilterTransactionsByCategory()
    {
        _context.Transactions.AddRange(
            new Transaction(100m, "Food item", DateTime.UtcNow, TransactionType.Expense, 1),
            new Transaction(200m, "Transport item", DateTime.UtcNow, TransactionType.Expense, 2)
        );
        await _context.SaveChangesAsync();

        var foodTransactions = await _context.Transactions
            .Where(t => t.CategoryId == 1)
            .ToListAsync();

        Assert.Single(foodTransactions);
        Assert.Equal("Food item", foodTransactions[0].Description);
    }

    [Fact]
    public async Task Query_CanFilterBudgetsByPeriod()
    {
        _context.Budgets.AddRange(
            new Budget(10000m, 10, 2026, 1),
            new Budget(15000m, 11, 2026, 2),
            new Budget(20000m, 11, 2026, 3)
        );
        await _context.SaveChangesAsync();

        var novBudgets = await _context.Budgets
            .Where(b => b.Year == 2026 && b.Month == 11)
            .ToListAsync();

        Assert.Equal(2, novBudgets.Count);
    }

    // ========================================================================
    // IAPPLICATIONDBCONTEXT INTERFACE TESTS
    // ========================================================================

    [Fact]
    public void DbContext_ImplementsIApplicationDbContext()
    {
        Assert.IsAssignableFrom<FinanceTrack.Application.Common.Interfaces.IApplicationDbContext>(_context);
    }

    [Fact]
    public async Task DbContext_SaveChangesAsyncWorks()
    {
        var transaction = new Transaction(
            100m, "Save test", DateTime.UtcNow,
            TransactionType.Expense, 1);

        _context.Transactions.Add(transaction);
        var result = await _context.SaveChangesAsync();

        Assert.True(result > 0);
    }

    // ========================================================================
    // TIMESTAMPS TEST
    // ========================================================================

    [Fact]
    public async Task Entity_CreatedAtIsSetAutomatically()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var transaction = new Transaction(
            100m, "Timestamp test", DateTime.UtcNow,
            TransactionType.Expense, 1);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        Assert.True(transaction.CreatedAt >= before);
    }
}
