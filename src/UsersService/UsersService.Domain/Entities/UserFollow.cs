namespace UsersService.Domain.Entities;

public sealed class UserFollow
{
    private UserFollow()
    {
    }

    private UserFollow(Guid followerUserId, Guid followedUserId, DateTime createdAtUtc)
    {
        if (followerUserId == Guid.Empty)
        {
            throw new ArgumentException("Follower user id is required.", nameof(followerUserId));
        }

        if (followedUserId == Guid.Empty)
        {
            throw new ArgumentException("Followed user id is required.", nameof(followedUserId));
        }

        if (followerUserId == followedUserId)
        {
            throw new ArgumentException("A user cannot follow themselves.");
        }

        FollowerUserId = followerUserId;
        FollowedUserId = followedUserId;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid FollowerUserId { get; private set; }
    public Guid FollowedUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static UserFollow Create(Guid followerUserId, Guid followedUserId)
    {
        return new UserFollow(followerUserId, followedUserId, DateTime.UtcNow);
    }
}
