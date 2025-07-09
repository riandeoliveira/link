using LinkJobber.Contexts;
using LinkJobber.Entities;
using LinkJobber.Interfaces;

namespace LinkJobber.Repositories;

public sealed class IgnoredJobRepository(AppDbContext context)
    : Repository<IgnoredJob>(context),
        IIgnoredJobRepository { }
