using Microsoft.EntityFrameworkCore;
using UsersService.Domain.Entities;
using UsersService.Domain.Repositories;
using UsersService.Infrastructure.Persistence;

namespace UsersService.Infrastructure.Repositories;

public sealed class UserRepository(UsersDbContext dbContext) : IUserRepository
{
    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.Trim();

        return await dbContext.Users
            .FirstOrDefaultAsync(user => user.UserName == normalizedUserName, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
