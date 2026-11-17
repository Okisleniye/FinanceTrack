using System.ComponentModel.DataAnnotations;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Enums;

namespace FinanceTrack.Web.Models;

public class CategoryFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category type is required.")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "Color is required.")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Please select a valid hex color code.")]
    public string Color { get; set; } = "#0d6efd";

    [Required(ErrorMessage = "Icon is required.")]
    [StringLength(50, ErrorMessage = "Icon identifier cannot exceed 50 characters.")]
    public string Icon { get; set; } = "bi-tag";

    public static CategoryFormModel FromDto(CategoryDto dto)
    {
        return new CategoryFormModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Type = dto.Type,
            Color = dto.Color,
            Icon = dto.Icon
        };
    }

    public CreateCategoryDto ToCreateDto()
    {
        return new CreateCategoryDto
        {
            Name = Name.Trim(),
            Type = Type,
            Color = Color.Trim(),
            Icon = Icon.Trim()
        };
    }

    public UpdateCategoryDto ToUpdateDto()
    {
        return new UpdateCategoryDto
        {
            Id = Id,
            Name = Name.Trim(),
            Type = Type,
            Color = Color.Trim(),
            Icon = Icon.Trim()
        };
    }
}
