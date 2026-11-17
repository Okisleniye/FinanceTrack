using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceTrack.IntegrationTests.Web;

public class TransactionsPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TransactionsPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }

    // ========================================================================
    // HTTP ENDPOINT / ROUTING TESTS
    // ========================================================================

    [Fact]
    public async Task GetTransactionsPage_ReturnsSuccessAndHtml()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/transactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Transactions", content);
        Assert.Contains("Add Transaction", content);
    }

    [Fact]
    public async Task GetTransactionsPage_ContainsToolbarAndFilterElements()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/transactions");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Search description...", content);
        Assert.Contains("All Types", content);
        Assert.Contains("All Categories", content);
        Assert.Contains("Newest First", content);
        Assert.Contains("Reset", content);
    }

    // ========================================================================
    // FORM MODEL VALIDATION TESTS
    // ========================================================================

    [Fact]
    public void TransactionFormModel_ValidModel_PassesValidation()
    {
        var model = new TransactionFormModel
        {
            Amount = 1500m,
            Description = "Valid Grocery Transaction",
            Date = DateTime.Today,
            Type = TransactionType.Expense,
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
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void TransactionFormModel_ZeroOrNegativeAmount_FailsValidation(decimal invalidAmount)
    {
        var model = new TransactionFormModel
        {
            Amount = invalidAmount,
            Description = "Test",
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            CategoryId = 1
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TransactionFormModel.Amount)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void TransactionFormModel_EmptyDescription_FailsValidation(string? invalidDesc)
    {
        var model = new TransactionFormModel
        {
            Amount = 100m,
            Description = invalidDesc!,
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            CategoryId = 1
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TransactionFormModel.Description)));
    }

    [Fact]
    public void TransactionFormModel_DescriptionExceeding200Chars_FailsValidation()
    {
        var model = new TransactionFormModel
        {
            Amount = 100m,
            Description = new string('A', 201),
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            CategoryId = 1
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TransactionFormModel.Description)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TransactionFormModel_InvalidCategory_FailsValidation(int invalidCategoryId)
    {
        var model = new TransactionFormModel
        {
            Amount = 100m,
            Description = "Test",
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            CategoryId = invalidCategoryId
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TransactionFormModel.CategoryId)));
    }

    // ========================================================================
    // FORM MODEL CONVERSION & DTO MAPPING TESTS
    // ========================================================================

    [Fact]
    public void TransactionFormModel_FromDto_MapsAllFieldsCorrectly()
    {
        var dto = new TransactionDto
        {
            Id = 42,
            Amount = 7500.50m,
            Description = "Monthly Internet Bill",
            Date = new DateTime(2026, 11, 10),
            Type = TransactionType.Expense,
            CategoryId = 4,
            CategoryName = "Housing",
            CategoryColor = "#0d6efd",
            CategoryIcon = "bi-house-door"
        };

        var model = TransactionFormModel.FromDto(dto);

        Assert.Equal(42, model.Id);
        Assert.Equal(7500.50m, model.Amount);
        Assert.Equal("Monthly Internet Bill", model.Description);
        Assert.Equal(new DateTime(2026, 11, 10), model.Date);
        Assert.Equal(TransactionType.Expense, model.Type);
        Assert.Equal(4, model.CategoryId);
    }

    [Fact]
    public void TransactionFormModel_ToCreateDto_MapsCorrectly()
    {
        var model = new TransactionFormModel
        {
            Amount = 50000m,
            Description = "Freelance Project Payment",
            Date = new DateTime(2026, 11, 15),
            Type = TransactionType.Income,
            CategoryId = 10
        };

        var createDto = model.ToCreateDto();

        Assert.Equal(50000m, createDto.Amount);
        Assert.Equal("Freelance Project Payment", createDto.Description);
        Assert.Equal(new DateTime(2026, 11, 15), createDto.Date);
        Assert.Equal(TransactionType.Income, createDto.Type);
        Assert.Equal(10, createDto.CategoryId);
    }

    [Fact]
    public void TransactionFormModel_ToUpdateDto_MapsCorrectly()
    {
        var model = new TransactionFormModel
        {
            Id = 15,
            Amount = 1200m,
            Description = "Updated Bus Pass",
            Date = new DateTime(2026, 11, 1),
            Type = TransactionType.Expense,
            CategoryId = 2
        };

        var updateDto = model.ToUpdateDto();

        Assert.Equal(15, updateDto.Id);
        Assert.Equal(1200m, updateDto.Amount);
        Assert.Equal("Updated Bus Pass", updateDto.Description);
        Assert.Equal(new DateTime(2026, 11, 1), updateDto.Date);
        Assert.Equal(TransactionType.Expense, updateDto.Type);
        Assert.Equal(2, updateDto.CategoryId);
    }

    // ========================================================================
    // WEB APPLICATION SERVICE INTEGRATION FLOW
    // ========================================================================

    [Fact]
    public async Task WebApp_CanResolveAndExecuteTransactionServiceFlow()
    {
        using var scope = _factory.Services.CreateScope();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

        Assert.NotNull(transactionService);
        Assert.NotNull(categoryService);

        // 1. Verify categories can be retrieved by type
        var expenseCategories = await categoryService.GetAllAsync(TransactionType.Expense);
        Assert.NotEmpty(expenseCategories);

        // 2. Create a new transaction through service
        var createDto = new CreateTransactionDto
        {
            Amount = 2500m,
            Description = "Integration Test Cafe Expense",
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            CategoryId = expenseCategories[0].Id
        };

        var created = await transactionService.CreateAsync(createDto);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(2500m, created.Amount);
        Assert.Equal("Integration Test Cafe Expense", created.Description);

        // 3. Query with filter matching description
        var filter = new TransactionFilterDto
        {
            SearchTerm = "Cafe Expense",
            Type = TransactionType.Expense
        };
        var paged = await transactionService.GetPagedAsync(filter);
        Assert.NotEmpty(paged.Items);
        Assert.Contains(paged.Items, t => t.Id == created.Id);

        // 4. Update the transaction
        var updateDto = new UpdateTransactionDto
        {
            Id = created.Id,
            Amount = 3000m,
            Description = "Updated Cafe Expense",
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            CategoryId = expenseCategories[0].Id
        };
        var updated = await transactionService.UpdateAsync(updateDto);
        Assert.Equal(3000m, updated.Amount);
        Assert.Equal("Updated Cafe Expense", updated.Description);

        // 5. Delete the transaction
        var deleted = await transactionService.DeleteAsync(created.Id);
        Assert.True(deleted);

        // 6. Verify lookup returns null
        var lookup = await transactionService.GetByIdAsync(created.Id);
        Assert.Null(lookup);
    }
}
