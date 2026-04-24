namespace SearchService.Domain.Entities;

public sealed record SearchUser(
    Guid Id,
    string Name,
    string UserName,
    string Email,
    string? ProfilePictureUrl,
    DateTime CreatedAtUtc);
