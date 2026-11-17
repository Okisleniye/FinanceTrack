using FinanceTrack.Domain.Common;
using FinanceTrack.Domain.Enums;
using FinanceTrack.Domain.Exceptions;

namespace FinanceTrack.Domain.Entities;

public class Category : BaseEntity, IAggregateRoot
{
    private readonly List<Transaction> _transactions = new();
    private readonly List<Budget> _budgets = new();

    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#0d6efd";
    public string Icon { get; private set; } = "bi-tag";
    public TransactionType Type { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    public IReadOnlyCollection<Budget> Budgets => _budgets.AsReadOnly();

    // Required for EF Core
    protected Category()
    {
    }

    public Category(string name, TransactionType type, string color = "#0d6efd", string icon = "bi-tag")
    {
        ValidateAndSet(name, type, color, icon);
    }

    public void Update(string name, TransactionType type, string color, string icon)
    {
        ValidateAndSet(name, type, color, icon);
        MarkUpdated();
    }

    public void AddTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction.Type != Type)
        {
            throw new DomainValidationException(
                $"Cannot add transaction of type '{transaction.Type}' to category '{Name}' of type '{Type}'.",
                nameof(Transaction.Type));
        }

        if (!_transactions.Contains(transaction))
        {
            _transactions.Add(transaction);
        }

        if (transaction.Category != this)
        {
            transaction.AssignCategory(this);
        }
    }

    public void AddBudget(Budget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        if (Type != TransactionType.Expense)
        {
            throw new DomainValidationException(
                $"Budgets can only be created for expense categories. Category '{Name}' is an income category.",
                nameof(Type));
        }

        if (HasBudgetFor(budget.Month, budget.Year, budget.Id))
        {
            throw new DomainValidationException(
                $"A budget for category '{Name}' already exists for {budget.Month}/{budget.Year}.",
                nameof(Budgets));
        }

        if (!_budgets.Contains(budget))
        {
            _budgets.Add(budget);
        }

        if (budget.Category != this)
        {
            budget.AssignCategory(this);
        }
    }

    public bool HasBudgetFor(int month, int year, int excludeBudgetId = 0)
    {
        return _budgets.Any(b => b.Month == month && b.Year == year && (excludeBudgetId == 0 || b.Id != excludeBudgetId));
    }

    private void ValidateAndSet(string name, TransactionType type, string color, string icon)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Category name cannot be empty.", nameof(Name));
        }

        if (name.Trim().Length > 50)
        {
            throw new DomainValidationException("Category name cannot exceed 50 characters.", nameof(Name));
        }

        if (!Enum.IsDefined(typeof(TransactionType), type))
        {
            throw new DomainValidationException("Invalid transaction type for category.", nameof(Type));
        }

        Name = name.Trim();
        Type = type;
        Color = string.IsNullOrWhiteSpace(color) ? "#0d6efd" : color.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? "bi-tag" : icon.Trim();
    }
}
