using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.Infrastructure.Data;

public class FinanceTrackDbContext : DbContext, IApplicationDbContext
{
    public FinanceTrackDbContext(DbContextOptions<FinanceTrackDbContext> options)
        : base(options)
    {
    }

    protected FinanceTrackDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceTrackDbContext).Assembly);
    }
}
