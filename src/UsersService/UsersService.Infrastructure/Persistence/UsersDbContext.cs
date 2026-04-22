using Microsoft.EntityFrameworkCore;
using UsersService.Domain.Entities;

namespace UsersService.Infrastructure.Persistence;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id)
                .ValueGeneratedNever();

            entity.Property(user => user.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(user => user.UserName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(user => user.ProfilePictureUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(user => user.CreatedAtUtc)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(user => user.UserName)
                .IsUnique();

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });
    }
}
