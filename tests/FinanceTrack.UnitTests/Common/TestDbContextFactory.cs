using System.Globalization;
using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.UnitTests.Common;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Budget> Budgets => Set<Budget>();

    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Color).HasMaxLength(30);
            builder.Property(c => c.Icon).HasMaxLength(30);
        });

        modelBuilder.Entity<Transaction>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Amount).HasPrecision(18, 2);
            builder.Property(t => t.Description).IsRequired().HasMaxLength(200);
            builder.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Budget>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Amount).HasPrecision(18, 2);
            builder.HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

public class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);
}

public static class TestDbContextFactory
{
    static TestDbContextFactory()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }

    public static (TestApplicationDbContext context, SqliteConnection connection) Create()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestApplicationDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
