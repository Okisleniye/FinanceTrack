using FinanceTrack.Application.Common.Models;
using FinanceTrack.Application.DTOs;

namespace FinanceTrack.Application.Common.Interfaces;

public interface ITransactionService
{
    Task<IReadOnlyList<TransactionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionFilterDto filter, CancellationToken cancellationToken = default);
    Task<TransactionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TransactionDto> CreateAsync(CreateTransactionDto dto, CancellationToken cancellationToken = default);
    Task<TransactionDto> UpdateAsync(UpdateTransactionDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
