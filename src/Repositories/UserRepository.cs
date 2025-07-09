using LinkJobber.Contexts;
using LinkJobber.Entities;
using LinkJobber.Interfaces;

namespace LinkJobber.Repositories;

public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository { }
