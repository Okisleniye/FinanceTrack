using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BudgetService(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<IReadOnlyList<BudgetDto>> GetAllAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default)
    {
        var targetYear = year ?? _dateTimeProvider.UtcNow.Year;
        var targetMonth = month ?? _dateTimeProvider.UtcNow.Month;

        var budgets = await _context.Budgets
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => (year == null || b.Year == targetYear) && (month == null || b.Month == targetMonth))
            .OrderBy(b => b.Category != null ? b.Category.Name : string.Empty)
            .ToListAsync(cancellationToken);

        var result = new List<BudgetDto>(budgets.Count);

        foreach (var b in budgets)
        {
            var spent = await CalculateSpentAsync(b.CategoryId, b.Month, b.Year, cancellationToken);
            result.Add(MapToDto(b, spent));
        }

        return result;
    }

    public async Task<BudgetDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var budget = await _context.Budgets
            .AsNoTracking()
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (budget == null)
        {
            return null;
        }

        var spent = await CalculateSpentAsync(budget.CategoryId, budget.Month, budget.Year, cancellationToken);
        return MapToDto(budget, spent);
    }

    public async Task<BudgetDto?> GetByPeriodAsync(int categoryId, int month, int year, CancellationToken cancellationToken = default)
    {
        var budget = await _context.Budgets
            .AsNoTracking()
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.CategoryId == categoryId && b.Month == month && b.Year == year, cancellationToken);

        if (budget == null)
        {
            return null;
        }

        var spent = await CalculateSpentAsync(budget.CategoryId, budget.Month, budget.Year, cancellationToken);
        return MapToDto(budget, spent);
    }

    public async Task<BudgetDto> CreateAsync(CreateBudgetDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), dto.CategoryId);
        }

        if (category.Type != TransactionType.Expense)
        {
            throw new ApplicationValidationException(
                $"Budgets can only be created for expense categories. Category '{category.Name}' is an income category.",
                nameof(dto.CategoryId));
        }

        var duplicate = await _context.Budgets
            .AnyAsync(b => b.CategoryId == dto.CategoryId && b.Month == dto.Month && b.Year == dto.Year, cancellationToken);

        if (duplicate)
        {
            throw new ApplicationValidationException(
                $"A budget for category '{category.Name}' in {dto.Month}/{dto.Year} already exists.",
                nameof(dto.Month));
        }

        var budget = new Budget(dto.Amount, dto.Month, dto.Year, dto.CategoryId);
        budget.AssignCategory(category);

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync(cancellationToken);

        var spent = await CalculateSpentAsync(budget.CategoryId, budget.Month, budget.Year, cancellationToken);
        return MapToDto(budget, spent);
    }

    public async Task<BudgetDto> UpdateAsync(UpdateBudgetDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var budget = await _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == dto.Id, cancellationToken);

        if (budget == null)
        {
            throw new NotFoundException(nameof(Budget), dto.Id);
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), dto.CategoryId);
        }

        if (category.Type != TransactionType.Expense)
        {
            throw new ApplicationValidationException(
                $"Budgets can only be assigned to expense categories. Category '{category.Name}' is an income category.",
                nameof(dto.CategoryId));
        }

        var duplicate = await _context.Budgets
            .AnyAsync(b => b.Id != dto.Id && b.CategoryId == dto.CategoryId && b.Month == dto.Month && b.Year == dto.Year, cancellationToken);

        if (duplicate)
        {
            throw new ApplicationValidationException(
                $"A budget for category '{category.Name}' in {dto.Month}/{dto.Year} already exists.",
                nameof(dto.Month));
        }

        budget.AssignCategory(category);
        budget.Update(dto.Amount, dto.Month, dto.Year, dto.CategoryId);

        await _context.SaveChangesAsync(cancellationToken);

        var spent = await CalculateSpentAsync(budget.CategoryId, budget.Month, budget.Year, cancellationToken);
        return MapToDto(budget, spent);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (budget == null)
        {
            return false;
        }

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<decimal> CalculateSpentAsync(int categoryId, int month, int year, CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.CategoryId == categoryId && t.Type == TransactionType.Expense && t.Date.Month == month && t.Date.Year == year)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;
    }

    private static BudgetDto MapToDto(Budget b, decimal spent)
    {
        return new BudgetDto
        {
            Id = b.Id,
            Amount = b.Amount,
            Month = b.Month,
            Year = b.Year,
            CategoryId = b.CategoryId,
            CategoryName = b.Category?.Name ?? string.Empty,
            CategoryColor = b.Category?.Color ?? "#0d6efd",
            CategoryIcon = b.Category?.Icon ?? "bi-piggy-bank",
            AmountSpent = spent
        };
    }
}
