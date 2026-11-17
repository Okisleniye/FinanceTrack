namespace FinanceTrack.Application.DTOs;

public record BudgetDto
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryColor { get; init; } = string.Empty;
    public string CategoryIcon { get; init; } = string.Empty;
    public decimal AmountSpent { get; init; }
    public decimal RemainingAmount => Amount - AmountSpent;
    public decimal PercentageUsed => Amount > 0 ? Math.Round((AmountSpent / Amount) * 100, 1) : 0;
    public bool IsOverBudget => AmountSpent > Amount;
}

public record CreateBudgetDto
{
    public decimal Amount { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public int CategoryId { get; init; }
}

public record UpdateBudgetDto
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public int CategoryId { get; init; }
}
