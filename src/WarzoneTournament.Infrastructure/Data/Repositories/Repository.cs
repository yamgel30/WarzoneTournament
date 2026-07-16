using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(e => !e.IsDeleted).ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(e => !e.IsDeleted).Where(predicate).ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsNoTrackingAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(e => !e.IsDeleted).Where(predicate).ToListAsync(ct);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(e => !e.IsDeleted).FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate, ct);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(e => !e.IsDeleted);
        return predicate != null ? await query.CountAsync(predicate, ct) : await query.CountAsync(ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await _dbSet.AddRangeAsync(entities, ct);

    public void Update(T entity) => AttachOrUpdate(entity);

    public void Remove(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        AttachOrUpdate(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities) Remove(entity);
    }

    public void HardRemove(T entity) => _dbSet.Remove(entity);

    // In Blazor Server the DbContext lives for the duration of the circuit. When
    // GetByIdAsync returns an AsNoTracking copy and the caller mutates + saves it,
    // _dbSet.Update() tries to attach the new copy but a tracked instance with the
    // same key may already exist → InvalidOperationException. Use SetValues instead.
    private void AttachOrUpdate(T entity)
    {
        var tracked = _context.ChangeTracker.Entries<T>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);
        if (tracked != null)
            tracked.CurrentValues.SetValues(entity);
        else
            _dbSet.Update(entity);
    }

    public void HardRemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

    public async Task<IReadOnlyList<T>> FindIncludingDeletedAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.IgnoreQueryFilters().Where(predicate).ToListAsync(ct);

    public IQueryable<T> Query() => _dbSet.Where(e => !e.IsDeleted).AsQueryable();
}
