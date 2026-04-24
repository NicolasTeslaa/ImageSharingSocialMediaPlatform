namespace SearchService.Application.DTOs;

public sealed record SearchResultDto(
    Guid Id,
    string Name,
    string UserName,
    string Email,
    string? ProfilePictureUrl,
    DateTime CreatedAtUtc);
