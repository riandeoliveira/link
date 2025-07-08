using LinkJoBot.Contexts;
using LinkJoBot.Entities;
using LinkJoBot.Interfaces;

namespace LinkJoBot.Repositories;

public sealed class UserRepository(AppDbContext context)
    : Repository<User>(context),
        IUserRepository
{ }
