using System.Security.Cryptography;
using System.Text;
using UsersService.Application.Abstractions;

namespace UsersService.Infrastructure.Security;

public sealed class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
