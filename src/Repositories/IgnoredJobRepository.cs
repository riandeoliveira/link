using LinkJoBot.Contexts;
using LinkJoBot.Entities;
using LinkJoBot.Interfaces;

namespace LinkJoBot.Repositories;

public sealed class IgnoredJobRepository(AppDbContext context)
    : Repository<IgnoredJob>(context),
        IIgnoredJobRepository
{ }
