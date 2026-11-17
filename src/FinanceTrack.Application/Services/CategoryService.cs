using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Entities;
using FinanceTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _context;

    public CategoryService(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(TransactionType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .AsNoTracking()
            .Include(c => c.Transactions)
            .Include(c => c.Budgets)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Transactions)
            .Include(c => c.Budgets)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return category != null ? MapToDto(category) : null;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var normalizedName = dto.Name.Trim().ToLower();
        var exists = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == normalizedName && c.Type == dto.Type, cancellationToken);

        if (exists)
        {
            throw new ApplicationValidationException(
                $"A category named '{dto.Name}' of type '{dto.Type}' already exists.",
                nameof(dto.Name));
        }

        var category = new Category(dto.Name, dto.Type, dto.Color, dto.Icon);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var category = await _context.Categories
            .Include(c => c.Transactions)
            .Include(c => c.Budgets)
            .FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), dto.Id);
        }

        var normalizedName = dto.Name.Trim().ToLower();
        var duplicate = await _context.Categories
            .AnyAsync(c => c.Id != dto.Id && c.Name.ToLower() == normalizedName && c.Type == dto.Type, cancellationToken);

        if (duplicate)
        {
            throw new ApplicationValidationException(
                $"Another category named '{dto.Name}' of type '{dto.Type}' already exists.",
                nameof(dto.Name));
        }

        if (category.Type != dto.Type && (category.Transactions.Any() || category.Budgets.Any()))
        {
            throw new ApplicationValidationException(
                "Cannot change category type when it contains existing transactions or budgets.",
                nameof(dto.Type));
        }

        category.Update(dto.Name, dto.Type, dto.Color, dto.Icon);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task<bool> CanDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var hasTransactions = await _context.Transactions.AnyAsync(t => t.CategoryId == id, cancellationToken);
        var hasBudgets = await _context.Budgets.AnyAsync(b => b.CategoryId == id, cancellationToken);

        return !hasTransactions && !hasBudgets;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
        {
            return false;
        }

        if (!await CanDeleteAsync(id, cancellationToken))
        {
            throw new ApplicationValidationException(
                $"Category '{category.Name}' cannot be deleted because it is currently used by existing transactions or budgets.",
                nameof(id));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static CategoryDto MapToDto(Category c)
    {
        return new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Color = c.Color,
            Icon = c.Icon,
            Type = c.Type,
            TransactionCount = c.Transactions.Count,
            BudgetCount = c.Budgets.Count
        };
    }
}
