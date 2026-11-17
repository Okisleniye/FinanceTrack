using FinanceTrack.Application.DTOs;

namespace FinanceTrack.Application.Common.Interfaces;

public interface IBudgetService
{
    Task<IReadOnlyList<BudgetDto>> GetAllAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default);
    Task<BudgetDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BudgetDto> CreateAsync(CreateBudgetDto dto, CancellationToken cancellationToken = default);
    Task<BudgetDto> UpdateAsync(UpdateBudgetDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<BudgetDto?> GetByPeriodAsync(int categoryId, int month, int year, CancellationToken cancellationToken = default);
}
