namespace UsersService.Application.DTOs;

public sealed record FollowResultDto(Guid FollowerUserId, Guid FollowedUserId, DateTime CreatedAtUtc);
