using FinanceTrack.Domain.Enums;

namespace FinanceTrack.Application.DTOs;

public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = "#0d6efd";
    public string Icon { get; init; } = "bi-tag";
    public TransactionType Type { get; init; }
    public string TypeName => Type.ToString();
    public int TransactionCount { get; init; }
    public int BudgetCount { get; init; }
}

public record CreateCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public TransactionType Type { get; init; } = TransactionType.Expense;
    public string Color { get; init; } = "#0d6efd";
    public string Icon { get; init; } = "bi-tag";
}

public record UpdateCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TransactionType Type { get; init; }
    public string Color { get; init; } = "#0d6efd";
    public string Icon { get; init; } = "bi-tag";
}
