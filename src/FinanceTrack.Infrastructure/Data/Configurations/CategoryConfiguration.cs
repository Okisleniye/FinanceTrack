using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTrack.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("#0d6efd");

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("bi-tag");

        builder.Property(c => c.Type)
            .IsRequired();

        builder.HasIndex(c => new { c.Name, c.Type });

        // Configure backing fields for collections
        builder.Navigation(c => c.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(c => c.Budgets)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Relationships
        builder.HasMany(c => c.Transactions)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Budgets)
            .WithOne(b => b.Category)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed default categories
        builder.HasData(
            // Expense Categories
            new { Id = 1, Name = "Food", Color = "#fd7e14", Icon = "bi-cup-hot", Type = TransactionType.Expense, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 2, Name = "Transport", Color = "#0dcaf0", Icon = "bi-car-front", Type = TransactionType.Expense, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 3, Name = "Shopping", Color = "#6f42c1", Icon = "bi-bag", Type = TransactionType.Expense, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 4, Name = "Housing", Color = "#0d6efd", Icon = "bi-house-door", Type = TransactionType.Expense, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 5, Name = "Entertainment", Color = "#d63384", Icon = "bi-controller", Type = TransactionType.Expense, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 6, Name = "Health", Color = "#dc3545", Icon = "bi-heart-pulse", Type = TransactionType.Expense, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 7, Name = "Education", Color = "#20c997", Icon = "bi-mortarboard", Type = TransactionType.Expense, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 8, Name = "Other", Color = "#6c757d", Icon = "bi-three-dots", Type = TransactionType.Expense, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },

            // Income Categories
            new { Id = 9, Name = "Salary", Color = "#198754", Icon = "bi-cash-stack", Type = TransactionType.Income, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 10, Name = "Freelance", Color = "#20c997", Icon = "bi-laptop", Type = TransactionType.Income, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 11, Name = "Business", Color = "#0d6efd", Icon = "bi-briefcase", Type = TransactionType.Income, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 12, Name = "Investments", Color = "#ffc107", Icon = "bi-graph-up-arrow", Type = TransactionType.Income, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null },
            new { Id = 13, Name = "Other Income", Color = "#17a2b8", Icon = "bi-wallet2", Type = TransactionType.Income, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = (DateTime?)null }
        );
    }
}
