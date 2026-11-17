using FinanceTrack.Domain.Common;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Domain.Exceptions;

namespace FinanceTrack.Domain.Entities;

public class Budget : BaseEntity, IAggregateRoot
{
    public decimal Amount { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }

    // Required for EF Core
    protected Budget()
    {
    }

    public Budget(decimal amount, int month, int year, int categoryId)
    {
        ValidateAndSet(amount, month, year, categoryId);
    }

    public Budget(decimal amount, int month, int year, Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        ValidateAndSet(amount, month, year, category.Id <= 0 ? 1 : category.Id);
        AssignCategory(category);
    }

    public void Update(decimal amount, int month, int year, int categoryId)
    {
        ValidateAndSet(amount, month, year, categoryId);
        MarkUpdated();
    }

    public void AssignCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (category.Type != TransactionType.Expense)
        {
            throw new DomainValidationException(
                $"Budgets can only be assigned to expense categories. Category '{category.Name}' is an income category.",
                nameof(Category));
        }

        if (category.Id != 0 && category.Id != CategoryId)
        {
            CategoryId = category.Id;
        }

        Category = category;
        MarkUpdated();
    }

    public bool HasConflictWith(int categoryId, int month, int year)
    {
        return CategoryId == categoryId && Month == month && Year == year;
    }

    private void ValidateAndSet(decimal amount, int month, int year, int categoryId)
    {
        if (amount <= 0)
        {
            throw new DomainValidationException("Budget amount must be greater than zero.", nameof(Amount));
        }

        if (month < 1 || month > 12)
        {
            throw new DomainValidationException("Budget month must be between 1 and 12.", nameof(Month));
        }

        if (year < 2000 || year > 2100)
        {
            throw new DomainValidationException("Budget year must be between 2000 and 2100.", nameof(Year));
        }

        if (categoryId <= 0)
        {
            throw new DomainValidationException("A valid category must be specified for the budget.", nameof(CategoryId));
        }

        Amount = amount;
        Month = month;
        Year = year;
        CategoryId = categoryId;
    }
}
