namespace SearchService.Application.DTOs;

public sealed record SearchUserUpsertRequest(
    Guid Id,
    string Name,
    string UserName,
    string Email,
    string? ProfilePictureUrl,
    DateTime CreatedAtUtc);
