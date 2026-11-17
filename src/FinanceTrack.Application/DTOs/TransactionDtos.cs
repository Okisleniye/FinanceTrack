using FinanceTrack.Domain.Enums;

namespace FinanceTrack.Application.DTOs;

public record TransactionDto
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public TransactionType Type { get; init; }
    public string TypeName => Type.ToString();
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryColor { get; init; } = string.Empty;
    public string CategoryIcon { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record CreateTransactionDto
{
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime Date { get; init; } = DateTime.UtcNow.Date;
    public TransactionType Type { get; init; } = TransactionType.Expense;
    public int CategoryId { get; init; }
}

public record UpdateTransactionDto
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public TransactionType Type { get; init; }
    public int CategoryId { get; init; }
}

public record TransactionFilterDto
{
    public string? SearchTerm { get; init; }
    public TransactionType? Type { get; init; }
    public int? CategoryId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? SortBy { get; init; } = "date_desc";
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
