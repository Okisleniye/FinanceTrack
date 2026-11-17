using FinanceTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTrack.Infrastructure.Data.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.Month)
            .IsRequired();

        builder.Property(b => b.Year)
            .IsRequired();

        builder.Property(b => b.CategoryId)
            .IsRequired();

        // Unique constraint on (CategoryId, Month, Year) to prevent duplicate budgets
        builder.HasIndex(b => new { b.CategoryId, b.Month, b.Year })
            .IsUnique();

        // Index on period for quick lookups
        builder.HasIndex(b => new { b.Year, b.Month });

        // Relationship
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Budgets)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
