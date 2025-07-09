using System.Linq.Expressions;
using LinkJobber.Contexts;
using LinkJobber.Entities;
using LinkJobber.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LinkJobber.Repositories;

public abstract class Repository<TEntity>(AppDbContext context) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly AppDbContext _context = context;

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken
    )
    {
        return await _context.Set<TEntity>().CountAsync(predicate, cancellationToken);
    }

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);

        return entity;
    }

    public async Task DeleteManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken
    )
    {
        var entities = await _context
            .Set<TEntity>()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        _context.Set<TEntity>().RemoveRange(entities);
    }

    public async Task<IEnumerable<TEntity>> FindManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken
    )
    {
        var entities = await _context
            .Set<TEntity>()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        return entities;
    }

    public async Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken
    )
    {
        var entity = await _context
            .Set<TEntity>()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        return entity;
    }

    public Task UpdateAsync(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<TEntity>().Update(entity);

        return Task.CompletedTask;
    }
}
