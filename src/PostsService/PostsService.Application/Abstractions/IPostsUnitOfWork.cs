namespace PostsService.Application.Abstractions;

public interface IPostsUnitOfWork
{
    Task ExecuteTransactionalAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);
}
