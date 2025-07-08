using System.Linq.Expressions;
using LinkJoBot.Entities;

namespace LinkJoBot.Interfaces;

public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken
    );

    public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);

    public Task DeleteManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken
    );

    public Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken
    );

    public Task<IEnumerable<TEntity>> FindManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken
    );

    public Task UpdateAsync(TEntity entity);
}
