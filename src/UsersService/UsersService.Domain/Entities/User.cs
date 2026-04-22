namespace UsersService.Domain.Entities;

public sealed class User
{
    private User()
    {
    }

    private User(
        Guid id,
        string name,
        string userName,
        string email,
        string passwordHash,
        string? profilePictureUrl,
        DateTime createdAtUtc)
    {
        Id = id;
        SetName(name);
        SetUserName(userName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetProfilePictureUrl(profilePictureUrl);
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string? ProfilePictureUrl { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public static User Create(
        string name,
        string userName,
        string email,
        string passwordHash,
        string? profilePictureUrl)
    {
        return new User(
            Guid.NewGuid(),
            name,
            userName,
            email,
            passwordHash,
            profilePictureUrl,
            DateTime.UtcNow);
    }

    public void Update(string name, string userName, string email, string? profilePictureUrl)
    {
        SetName(name);
        SetUserName(userName);
        SetEmail(email);
        SetProfilePictureUrl(profilePictureUrl);
    }

    public void UpdatePassword(string passwordHash)
    {
        SetPasswordHash(passwordHash);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("Username is required.", nameof(userName));
        }

        UserName = userName.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        PasswordHash = passwordHash;
    }

    private void SetProfilePictureUrl(string? profilePictureUrl)
    {
        if (string.IsNullOrWhiteSpace(profilePictureUrl))
        {
            ProfilePictureUrl = null;
            return;
        }

        if (!Uri.TryCreate(profilePictureUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Profile picture URL must be a valid absolute URL.", nameof(profilePictureUrl));
        }

        ProfilePictureUrl = profilePictureUrl.Trim();
    }
}
