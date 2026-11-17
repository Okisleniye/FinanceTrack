using FinanceTrack.Application.Common.Exceptions;
using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Application.Common.Models;
using FinanceTrack.Application.DTOs;
using FinanceTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IApplicationDbContext _context;

    public TransactionService(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<TransactionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(t => t.Description.ToLower().Contains(search) 
                                  || (t.Category != null && t.Category.Name.ToLower().Contains(search)));
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(t => t.Type == filter.Type.Value);
        }

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(t => t.Date >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(t => t.Date <= filter.EndDate.Value);
        }

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "date_asc" => query.OrderBy(t => t.Date).ThenBy(t => t.Id),
            "amount_desc" => query.OrderByDescending(t => t.Amount).ThenByDescending(t => t.Date),
            "amount_asc" => query.OrderBy(t => t.Amount).ThenBy(t => t.Date),
            _ => query.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id)
        };

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<TransactionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return transaction != null ? MapToDto(transaction) : null;
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), dto.CategoryId);
        }

        if (category.Type != dto.Type)
        {
            throw new ApplicationValidationException(
                $"Transaction type '{dto.Type}' is incompatible with category '{category.Name}' ({category.Type}).",
                nameof(dto.Type));
        }

        var transaction = new Transaction(dto.Amount, dto.Description, dto.Date, dto.Type, dto.CategoryId);
        transaction.AssignCategory(category);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(transaction);
    }

    public async Task<TransactionDto> UpdateAsync(UpdateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == dto.Id, cancellationToken);

        if (transaction == null)
        {
            throw new NotFoundException(nameof(Transaction), dto.Id);
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), dto.CategoryId);
        }

        if (category.Type != dto.Type)
        {
            throw new ApplicationValidationException(
                $"Transaction type '{dto.Type}' is incompatible with category '{category.Name}' ({category.Type}).",
                nameof(dto.Type));
        }

        transaction.AssignCategory(category);
        transaction.Update(dto.Amount, dto.Description, dto.Date, dto.Type, dto.CategoryId);

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(transaction);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (transaction == null)
        {
            return false;
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static TransactionDto MapToDto(Transaction t)
    {
        return new TransactionDto
        {
            Id = t.Id,
            Amount = t.Amount,
            Description = t.Description,
            Date = t.Date,
            Type = t.Type,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? string.Empty,
            CategoryColor = t.Category?.Color ?? "#0d6efd",
            CategoryIcon = t.Category?.Icon ?? "bi-tag",
            CreatedAt = t.CreatedAt
        };
    }
}
