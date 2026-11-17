using FinanceTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Transaction> Transactions { get; }
    DbSet<Category> Categories { get; }
    DbSet<Budget> Budgets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
