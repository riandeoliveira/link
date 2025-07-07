using JobScraperBot.Contexts;
using JobScraperBot.Interfaces;
using JobScraperBot.Models;

namespace JobScraperBot.Repositories;

public sealed class IgnoredJobRepository(
    AppDbContext context
) : Repository<IgnoredJob>(context), IIgnoredJobRepository
{
}
