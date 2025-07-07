using System.Linq.Expressions;

using JobScraperBot.Models;

namespace JobScraperBot.Interfaces;

public interface IRepository<TModel> where TModel : BaseModel
{
    public Task<int> CountAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken);

    public Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken);

    public Task DeleteManyAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken);

    public Task<bool> ExistAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken);

    public Task<TModel?> FindOneAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken);

    public Task<IEnumerable<TModel>> FindManyAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken);

    public Task UpdateAsync(TModel model);
}
