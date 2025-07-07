using System.Linq.Expressions;

using JobScraperBot.Contexts;
using JobScraperBot.Interfaces;
using JobScraperBot.Models;

using Microsoft.EntityFrameworkCore;

namespace JobScraperBot.Repositories;

public abstract class Repository<TModel>(
    AppDbContext context
) : IRepository<TModel> where TModel : BaseModel
{
    public async Task<int> CountAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken)
    {
        return await context.Set<TModel>().CountAsync(predicate, cancellationToken);
    }

    public async Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken)
    {
        await context.Set<TModel>().AddAsync(model, cancellationToken);

        return model;
    }

    public async Task DeleteManyAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken)
    {
        IEnumerable<TModel> models = await context.Set<TModel>()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        context.Set<TModel>().RemoveRange(models);
    }

    public async Task<bool> ExistAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken)
    {
        TModel? model = await context.Set<TModel>()
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        return model is not null;
    }

    public async Task<IEnumerable<TModel>> FindManyAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken)
    {
        IEnumerable<TModel> models = await context.Set<TModel>()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        return models;
    }

    public async Task<TModel?> FindOneAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken)
    {
        TModel? model = await context.Set<TModel>()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        return model;
    }

    public Task UpdateAsync(TModel model)
    {
        model.UpdatedAt = DateTime.UtcNow;

        context.Set<TModel>().Update(model);

        return Task.CompletedTask;
    }
}
