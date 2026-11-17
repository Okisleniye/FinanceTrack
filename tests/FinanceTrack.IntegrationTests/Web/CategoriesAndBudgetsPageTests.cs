using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceTrack.IntegrationTests.Web;

public class CategoriesAndBudgetsPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CategoriesAndBudgetsPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }

    // ========================================================================
    // HTTP ENDPOINT / ROUTING TESTS
    // ========================================================================

    [Fact]
    public async Task GetCategoriesPage_ReturnsSuccessAndHtml()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Categories", content);
        Assert.Contains("Add Category", content);
        Assert.Contains("Expenses", content);
        Assert.Contains("Income", content);
    }

    [Fact]
    public async Task GetBudgetsPage_ReturnsSuccessAndHtml()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/budgets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Monthly Budgets", content);
        Assert.Contains("Set Budget", content);
        Assert.Contains("Total Budgeted", content);
        Assert.Contains("Total Spent", content);
    }

    // ========================================================================
    // CATEGORY FORM MODEL VALIDATION & MAPPING
    // ========================================================================

    [Fact]
    public void CategoryFormModel_ValidModel_PassesValidation()
    {
        var model = new CategoryFormModel
        {
            Name = "Books & Media",
            Type = TransactionType.Expense,
            Color = "#6f42c1",
            Icon = "bi-book"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CategoryFormModel_EmptyName_FailsValidation(string? invalidName)
    {
        var model = new CategoryFormModel
        {
            Name = invalidName!,
            Type = TransactionType.Expense,
            Color = "#6f42c1",
            Icon = "bi-tag"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CategoryFormModel.Name)));
    }

    [Fact]
    public void CategoryFormModel_NameExceeding50Chars_FailsValidation()
    {
        var model = new CategoryFormModel
        {
            Name = new string('C', 51),
            Type = TransactionType.Expense,
            Color = "#6f42c1",
            Icon = "bi-tag"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CategoryFormModel.Name)));
    }

    [Theory]
    [InlineData("not-a-color")]
    [InlineData("#12")]
    [InlineData("123456")]
    public void CategoryFormModel_InvalidHexColor_FailsValidation(string invalidColor)
    {
        var model = new CategoryFormModel
        {
            Name = "Valid Name",
            Type = TransactionType.Expense,
            Color = invalidColor,
            Icon = "bi-tag"
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CategoryFormModel.Color)));
    }

    [Fact]
    public void CategoryFormModel_MappingMethods_WorkCorrectly()
    {
        var dto = new CategoryDto
        {
            Id = 5,
            Name = "Entertainment",
            Type = TransactionType.Expense,
            Color = "#d63384",
            Icon = "bi-controller"
        };

        var model = CategoryFormModel.FromDto(dto);
        Assert.Equal(5, model.Id);
        Assert.Equal("Entertainment", model.Name);
        Assert.Equal(TransactionType.Expense, model.Type);
        Assert.Equal("#d63384", model.Color);

        var createDto = model.ToCreateDto();
        Assert.Equal("Entertainment", createDto.Name);

        var updateDto = model.ToUpdateDto();
        Assert.Equal(5, updateDto.Id);
        Assert.Equal("Entertainment", updateDto.Name);
    }

    // ========================================================================
    // BUDGET FORM MODEL VALIDATION & MAPPING
    // ========================================================================

    [Fact]
    public void BudgetFormModel_ValidModel_PassesValidation()
    {
        var model = new BudgetFormModel
        {
            Amount = 45000m,
            Month = 11,
            Year = 2026,
            CategoryId = 1
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void BudgetFormModel_NonPositiveAmount_FailsValidation(decimal invalidAmount)
    {
        var model = new BudgetFormModel
        {
            Amount = invalidAmount,
            Month = 11,
            Year = 2026,
            CategoryId = 1
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BudgetFormModel.Amount)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void BudgetFormModel_InvalidMonth_FailsValidation(int invalidMonth)
    {
        var model = new BudgetFormModel
        {
            Amount = 10000m,
            Month = invalidMonth,
            Year = 2026,
            CategoryId = 1
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BudgetFormModel.Month)));
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void BudgetFormModel_InvalidYear_FailsValidation(int invalidYear)
    {
        var model = new BudgetFormModel
        {
            Amount = 10000m,
            Month = 6,
            Year = invalidYear,
            CategoryId = 1
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BudgetFormModel.Year)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BudgetFormModel_InvalidCategory_FailsValidation(int invalidCategoryId)
    {
        var model = new BudgetFormModel
        {
            Amount = 10000m,
            Month = 6,
            Year = 2026,
            CategoryId = invalidCategoryId
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BudgetFormModel.CategoryId)));
    }

    // ========================================================================
    // END-TO-END WEB SERVICE FLOW TESTS
    // ========================================================================

    [Fact]
    public async Task WebApp_CategoryServiceFlow_CreateUpdateAndEnforceDeleteRules()
    {
        using var scope = _factory.Services.CreateScope();
        var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        // 1. Create a unique custom category
        var createDto = new CreateCategoryDto
        {
            Name = "Integration Custom Exp",
            Type = TransactionType.Expense,
            Color = "#20c997",
            Icon = "bi-star"
        };
        var created = await categoryService.CreateAsync(createDto);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        // 2. Can delete before any transactions are added
        var canDelete = await categoryService.CanDeleteAsync(created.Id);
        Assert.True(canDelete);

        // 3. Add a transaction referencing this category
        var txnDto = new CreateTransactionDto
        {
            Amount = 500m,
            Description = "Expense linking test",
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            CategoryId = created.Id
        };
        var createdTxn = await transactionService.CreateAsync(txnDto);

        // 4. Verify CanDeleteAsync is now false and delete throws ApplicationValidationException
        var canDeleteWithTxn = await categoryService.CanDeleteAsync(created.Id);
        Assert.False(canDeleteWithTxn);

        await Assert.ThrowsAsync<ApplicationValidationException>(
            () => categoryService.DeleteAsync(created.Id));

        // 5. Clean up: Delete transaction first, then delete category successfully
        await transactionService.DeleteAsync(createdTxn.Id);

        var canDeleteAfterCleanup = await categoryService.CanDeleteAsync(created.Id);
        Assert.True(canDeleteAfterCleanup);

        var deleted = await categoryService.DeleteAsync(created.Id);
        Assert.True(deleted);
    }

    [Fact]
    public async Task WebApp_BudgetServiceFlow_CalculatesRemainingAndPreventsDuplicates()
    {
        using var scope = _factory.Services.CreateScope();
        var budgetService = scope.ServiceProvider.GetRequiredService<IBudgetService>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        // Choose period far in future to isolate from other tests
        int testMonth = 7;
        int testYear = 2035;
        int expenseCategoryId = 1; // Food

        // 1. Create budget
        var createBudget = new CreateBudgetDto
        {
            Amount = 10000m,
            Month = testMonth,
            Year = testYear,
            CategoryId = expenseCategoryId
        };
        var budget = await budgetService.CreateAsync(createBudget);
        Assert.NotNull(budget);
        Assert.Equal(10000m, budget.Amount);
        Assert.Equal(0m, budget.AmountSpent);
        Assert.Equal(10000m, budget.RemainingAmount);
        Assert.False(budget.IsOverBudget);

        // 2. Add an expense transaction in this period
        var txn = await transactionService.CreateAsync(new CreateTransactionDto
        {
            Amount = 3500m,
            Description = "Budget spent test",
            Date = new DateTime(testYear, testMonth, 10),
            Type = TransactionType.Expense,
            CategoryId = expenseCategoryId
        });

        // 3. Reload budget and verify spent, remaining, and percentage
        var updatedBudget = await budgetService.GetByIdAsync(budget.Id);
        Assert.NotNull(updatedBudget);
        Assert.Equal(3500m, updatedBudget.AmountSpent);
        Assert.Equal(6500m, updatedBudget.RemainingAmount);
        Assert.Equal(35.0m, updatedBudget.PercentageUsed);

        // 4. Verify duplicate budget prevention for same category, month, and year
        await Assert.ThrowsAsync<ApplicationValidationException>(
            () => budgetService.CreateAsync(new CreateBudgetDto
            {
                Amount = 15000m,
                Month = testMonth,
                Year = testYear,
                CategoryId = expenseCategoryId
            }));

        // Clean up
        await transactionService.DeleteAsync(txn.Id);
        await budgetService.DeleteAsync(budget.Id);
    }
}
