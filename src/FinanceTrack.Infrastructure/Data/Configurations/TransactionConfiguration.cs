using FinanceTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTrack.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Date)
            .IsRequired();

        builder.Property(t => t.Type)
            .IsRequired();

        builder.Property(t => t.CategoryId)
            .IsRequired();

        // Useful indexes for querying, sorting, and reporting
        builder.HasIndex(t => t.Date);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => new { t.CategoryId, t.Date });

        // Relationship
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
