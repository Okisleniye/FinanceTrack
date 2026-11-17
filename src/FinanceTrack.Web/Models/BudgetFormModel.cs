using System.ComponentModel.DataAnnotations;
using FinanceTrack.Application.DTOs;

namespace FinanceTrack.Web.Models;

public class BudgetFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Budget amount is required.")]
    [Range(0.01, 1_000_000_000, ErrorMessage = "Budget amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Month is required.")]
    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
    public int Month { get; set; } = DateTime.Today.Month;

    [Required(ErrorMessage = "Year is required.")]
    [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100.")]
    public int Year { get; set; } = DateTime.Today.Year;

    [Required(ErrorMessage = "Please select a category.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an expense category.")]
    public int CategoryId { get; set; }

    public static BudgetFormModel FromDto(BudgetDto dto)
    {
        return new BudgetFormModel
        {
            Id = dto.Id,
            Amount = dto.Amount,
            Month = dto.Month,
            Year = dto.Year,
            CategoryId = dto.CategoryId
        };
    }

    public CreateBudgetDto ToCreateDto()
    {
        return new CreateBudgetDto
        {
            Amount = Amount,
            Month = Month,
            Year = Year,
            CategoryId = CategoryId
        };
    }

    public UpdateBudgetDto ToUpdateDto()
    {
        return new UpdateBudgetDto
        {
            Id = Id,
            Amount = Amount,
            Month = Month,
            Year = Year,
            CategoryId = CategoryId
        };
    }
}
