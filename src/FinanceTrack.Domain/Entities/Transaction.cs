using FinanceTrack.Domain.Common;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Domain.Exceptions;

namespace FinanceTrack.Domain.Entities;

public class Transaction : BaseEntity, IAggregateRoot
{
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public TransactionType Type { get; private set; }
    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }

    // Required for EF Core
    protected Transaction()
    {
    }

    public Transaction(
        decimal amount,
        string description,
        DateTime date,
        TransactionType type,
        int categoryId)
    {
        ValidateAndSet(amount, description, date, type, categoryId);
    }

    public Transaction(
        decimal amount,
        string description,
        DateTime date,
        Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        ValidateAndSet(amount, description, date, category.Type, category.Id <= 0 ? 1 : category.Id);
        Category = category;
        CategoryId = category.Id;
    }

    public void Update(
        decimal amount,
        string description,
        DateTime date,
        TransactionType type,
        int categoryId)
    {
        if (Category != null && Category.Id == categoryId && Category.Type != type)
        {
            throw new DomainValidationException(
                $"Transaction type '{type}' is incompatible with category '{Category.Name}' of type '{Category.Type}'.",
                nameof(Type));
        }

        ValidateAndSet(amount, description, date, type, categoryId);
        MarkUpdated();
    }

    public void AssignCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (category.Type != Type)
        {
            throw new DomainValidationException(
                $"Transaction type '{Type}' is incompatible with category '{category.Name}' of type '{category.Type}'.",
                nameof(Type));
        }

        if (category.Id != 0 && category.Id != CategoryId)
        {
            CategoryId = category.Id;
        }

        Category = category;
        MarkUpdated();
    }

    private void ValidateAndSet(
        decimal amount,
        string description,
        DateTime date,
        TransactionType type,
        int categoryId)
    {
        if (amount <= 0)
        {
            throw new DomainValidationException("Transaction amount must be greater than zero.", nameof(Amount));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException("Transaction description cannot be empty.", nameof(Description));
        }

        if (description.Trim().Length > 200)
        {
            throw new DomainValidationException("Transaction description cannot exceed 200 characters.", nameof(Description));
        }

        if (date == default)
        {
            throw new DomainValidationException("Transaction date is invalid.", nameof(Date));
        }

        if (!Enum.IsDefined(typeof(TransactionType), type))
        {
            throw new DomainValidationException("Invalid transaction type.", nameof(Type));
        }

        if (categoryId <= 0)
        {
            throw new DomainValidationException("A valid category must be specified for the transaction.", nameof(CategoryId));
        }

        Amount = amount;
        Description = description.Trim();
        Date = date;
        Type = type;
        CategoryId = categoryId;
    }
}
