using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Enums;

namespace FinanceTrack.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(TransactionType? type = null, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CanDeleteAsync(int id, CancellationToken cancellationToken = default);
}
