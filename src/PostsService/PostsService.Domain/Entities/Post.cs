using PostsService.Domain.Enums;

namespace PostsService.Domain.Entities;

public sealed class Post
{
    private Post()
    {
    }

    private Post(Guid id, Guid userId, string objectKey, string postUrl, PostType postType, DateTime createdAtUtc)
    {
        Id = id;
        UserId = userId;
        SetObjectKey(objectKey);
        SetPostUrl(postUrl);
        SetPostType(postType);
        TimestampUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ObjectKey { get; private set; } = string.Empty;
    public string PostUrl { get; private set; } = string.Empty;
    public DateTime TimestampUtc { get; private set; }
    public PostType PostType { get; private set; }

    public static Post Create(Guid userId, string objectKey, string postUrl, PostType postType, Guid? id = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        return new Post(id ?? Guid.NewGuid(), userId, objectKey, postUrl, postType, DateTime.UtcNow);
    }

    public void Update(string objectKey, string postUrl, PostType postType)
    {
        SetObjectKey(objectKey);
        SetPostUrl(postUrl);
        SetPostType(postType);
    }

    private void SetObjectKey(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ArgumentException("Object key is required.", nameof(objectKey));
        }

        ObjectKey = objectKey.Trim();
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
