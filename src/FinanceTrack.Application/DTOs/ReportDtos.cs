namespace FinanceTrack.Application.DTOs;

public record MonthlyTrendDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = string.Empty;
    public decimal Income { get; init; }
    public decimal Expenses { get; init; }
    public decimal NetSavings => Income - Expenses;
}

public record FinancialReportDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetSavings => TotalIncome - TotalExpenses;
    public decimal SavingsRate => TotalIncome > 0 ? Math.Round((NetSavings / TotalIncome) * 100, 1) : 0;
    public IReadOnlyList<CategorySpendingDto> ExpensesByCategory { get; init; } = Array.Empty<CategorySpendingDto>();
    public IReadOnlyList<CategorySpendingDto> IncomeByCategory { get; init; } = Array.Empty<CategorySpendingDto>();
    public IReadOnlyList<MonthlyTrendDto> MonthlyTrends { get; init; } = Array.Empty<MonthlyTrendDto>();
}
