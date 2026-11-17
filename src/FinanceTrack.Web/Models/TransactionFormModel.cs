using System.ComponentModel.DataAnnotations;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Enums;

namespace FinanceTrack.Web.Models;

public class TransactionFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 1_000_000_000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required.")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Transaction type is required.")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "Please select a category.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    public int CategoryId { get; set; }

    public static TransactionFormModel FromDto(TransactionDto dto)
    {
        return new TransactionFormModel
        {
            Id = dto.Id,
            Amount = dto.Amount,
            Description = dto.Description,
            Date = dto.Date,
            Type = dto.Type,
            CategoryId = dto.CategoryId
        };
    }

    public CreateTransactionDto ToCreateDto()
    {
        return new CreateTransactionDto
        {
            Amount = Amount,
            Description = Description.Trim(),
            Date = Date,
            Type = Type,
            CategoryId = CategoryId
        };
    }

    public UpdateTransactionDto ToUpdateDto()
    {
        return new UpdateTransactionDto
        {
            Id = Id,
            Amount = Amount,
            Description = Description.Trim(),
            Date = Date,
            Type = Type,
            CategoryId = CategoryId
        };
    }
}
