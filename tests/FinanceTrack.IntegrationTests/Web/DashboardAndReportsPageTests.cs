using System.Globalization;
using System.Net;
using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceTrack.IntegrationTests.Web;

public class DashboardAndReportsPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DashboardAndReportsPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }

    // ========================================================================
    // HTTP ENDPOINT / ROUTING TESTS
    // ========================================================================

    [Fact]
    public async Task GetDashboardPage_ReturnsSuccessAndHtml()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Financial Dashboard", content);
        Assert.Contains("Month Income", content);
        Assert.Contains("Month Expenses", content);
        Assert.Contains("Month Balance", content);
    }

    [Fact]
    public async Task GetReportsPage_ReturnsSuccessAndHtml()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/reports");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Reports & Analytics", content);
        Assert.Contains("Total Income", content);
        Assert.Contains("Total Expenses", content);
        Assert.Contains("Net Savings", content);
    }

    // ========================================================================
    // CONTAINER SERVICE EXECUTION TESTS
    // ========================================================================

    [Fact]
    public async Task WebApp_DashboardService_CalculatesAccurateSummary()
    {
        using var scope = _factory.Services.CreateScope();
        var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        // Add a test income and expense transaction in current month
        var incomeTxn = await transactionService.CreateAsync(new CreateTransactionDto
        {
            Amount = 100000m,
            Description = "Dashboard Integration Income",
            Date = DateTime.UtcNow,
            Type = TransactionType.Income,
            CategoryId = 9 // Salary
        });

        var expenseTxn = await transactionService.CreateAsync(new CreateTransactionDto
        {
            Amount = 25000m,
            Description = "Dashboard Integration Expense",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense,
            CategoryId = 1 // Food
        });

        var summary = await dashboardService.GetSummaryAsync();

        Assert.NotNull(summary);
        Assert.True(summary.CurrentMonthIncome >= 100000m);
        Assert.True(summary.CurrentMonthExpenses >= 25000m);
        Assert.Equal(summary.CurrentMonthIncome - summary.CurrentMonthExpenses, summary.CurrentMonthBalance);
        Assert.NotEmpty(summary.RecentTransactions);
        Assert.NotEmpty(summary.SpendingByCategory);

        // Clean up
        await transactionService.DeleteAsync(incomeTxn.Id);
        await transactionService.DeleteAsync(expenseTxn.Id);
    }

    [Fact]
    public async Task WebApp_ReportService_CalculatesMetricsAndTrends()
    {
        using var scope = _factory.Services.CreateScope();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var txn = await transactionService.CreateAsync(new CreateTransactionDto
        {
            Amount = 12000m,
            Description = "Report Integration Test",
            Date = start.AddDays(2),
            Type = TransactionType.Expense,
            CategoryId = 2 // Transport
        });

        var report = await reportService.GetReportAsync(start, end);

        Assert.NotNull(report);
        Assert.True(report.TotalExpenses >= 12000m);
        Assert.Equal(report.TotalIncome - report.TotalExpenses, report.NetSavings);

        var trends = await reportService.GetMonthlyTrendsAsync(6);
        Assert.NotNull(trends);
        Assert.Equal(6, trends.Count);

        // Clean up
        await transactionService.DeleteAsync(txn.Id);
    }
}
