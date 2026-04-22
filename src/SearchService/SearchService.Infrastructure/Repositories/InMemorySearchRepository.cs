using SearchService.Domain.Entities;
using SearchService.Domain.Repositories;

namespace SearchService.Infrastructure.Repositories;

public sealed class InMemorySearchRepository : ISearchRepository
{
    private static readonly IReadOnlyCollection<SearchResult> Results =
    [
        new(Guid.NewGuid(), "post", "Por do sol em Sao Paulo", "Colecao de imagens urbanas ao entardecer."),
        new(Guid.NewGuid(), "user", "ana.dev", "Creator focada em retratos e viagens."),
        new(Guid.NewGuid(), "tag", "#streetphotography", "Conteudo em alta na plataforma.")
    ];

    public Task<IReadOnlyCollection<SearchResult>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Task.FromResult(Results);
        }

        var filtered = Results
            .Where(result =>
                result.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                result.Snippet.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                result.Type.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<SearchResult>>(filtered);
    }
}
