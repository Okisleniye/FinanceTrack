namespace FinanceTrack.Application.DTOs;

public record CategorySpendingDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Color { get; init; } = "#0d6efd";
    public string Icon { get; init; } = "bi-tag";
    public decimal TotalAmount { get; init; }
    public decimal Percentage { get; init; }
}

public record DashboardSummaryDto
{
    public decimal TotalBalance => TotalIncome - TotalExpenses;
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal CurrentMonthIncome { get; init; }
    public decimal CurrentMonthExpenses { get; init; }
    public decimal CurrentMonthBalance => CurrentMonthIncome - CurrentMonthExpenses;
    public IReadOnlyList<TransactionDto> RecentTransactions { get; init; } = Array.Empty<TransactionDto>();
    public IReadOnlyList<CategorySpendingDto> SpendingByCategory { get; init; } = Array.Empty<CategorySpendingDto>();
    public IReadOnlyList<BudgetDto> BudgetOverview { get; init; } = Array.Empty<BudgetDto>();
}
