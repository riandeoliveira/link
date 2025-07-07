using JobScraperBot.Contexts;
using JobScraperBot.Interfaces;
using JobScraperBot.Models;

namespace JobScraperBot.Repositories;

public sealed class UserRepository(
    AppDbContext context
) : Repository<User>(context), IUserRepository
{
}
