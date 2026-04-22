using PostsService.Domain.Enums;

namespace PostsService.Domain.Entities;

public sealed class Post
{
    private Post()
    {
    }

    private Post(Guid id, Guid userId, string postUrl, PostType postType, DateTime createdAtUtc)
    {
        Id = id;
        UserId = userId;
        SetPostUrl(postUrl);
        SetPostType(postType);
        TimestampUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PostUrl { get; private set; } = string.Empty;
    public DateTime TimestampUtc { get; private set; }
    public PostType PostType { get; private set; }

    public static Post Create(Guid userId, string postUrl, PostType postType)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        return new Post(Guid.NewGuid(), userId, postUrl, postType, DateTime.UtcNow);
    }

    public void Update(string postUrl, PostType postType)
    {
        SetPostUrl(postUrl);
        SetPostType(postType);
    }

    private void SetPostUrl(string postUrl)
    {
        if (string.IsNullOrWhiteSpace(postUrl))
        {
            throw new ArgumentException("Post URL is required.", nameof(postUrl));
        }

        if (!Uri.TryCreate(postUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Post URL must be a valid absolute URL.", nameof(postUrl));
        }

        PostUrl = postUrl.Trim();
    }

    private void SetPostType(PostType postType)
    {
        if (!Enum.IsDefined(postType))
        {
            throw new ArgumentException("Post type is invalid.", nameof(postType));
        }

        PostType = postType;
    }
}
