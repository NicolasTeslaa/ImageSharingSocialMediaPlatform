using APIGateway.Application;
using APIGateway.Application.Abstractions;
using APIGateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGatewayApplication()
    .AddGatewayInfrastructure();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Scheme is "http" or "https"
                    && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                        || uri.Host.Equals("127.0.0.1"));
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("FrontendDev");

app.MapGet("/health", () => Results.Ok(new { service = "APIGateway", status = "Healthy" }));

app.MapGet("/gateway/routes", async (IRouteQueryService queryService, CancellationToken cancellationToken) =>
{
    var routes = await queryService.GetRoutesAsync(cancellationToken);
    return Results.Ok(routes);
});

var proxyMethods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE" };

app.MapMethods("/users/{userId:guid}/posts", proxyMethods, ProxyToPostsServiceAsync);
app.MapMethods("/users", proxyMethods, ProxyToUsersServiceAsync);
app.MapMethods("/users/{**path}", proxyMethods, ProxyToUsersServiceAsync);
app.MapMethods("/auth", proxyMethods, ProxyToUsersServiceAsync);
app.MapMethods("/auth/{**path}", proxyMethods, ProxyToUsersServiceAsync);
app.MapMethods("/internal", proxyMethods, ProxyToUsersServiceAsync);
app.MapMethods("/internal/{**path}", proxyMethods, ProxyToUsersServiceAsync);
app.MapMethods("/search", proxyMethods, ProxyToSearchServiceAsync);
app.MapMethods("/search/{**path}", proxyMethods, ProxyToSearchServiceAsync);
app.MapMethods("/timeline", proxyMethods, ProxyToTimelineServiceAsync);
app.MapMethods("/timeline/{**path}", proxyMethods, ProxyToTimelineServiceAsync);
app.MapMethods("/posts", proxyMethods, ProxyToPostsServiceAsync);
app.MapMethods("/posts/{**path}", proxyMethods, ProxyToPostsServiceAsync);

app.Run();

static Task<IResult> ProxyToUsersServiceAsync(
    HttpContext httpContext,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
    ProxyAsync(httpContext, httpClientFactory, "http://localhost:5166", cancellationToken);

static Task<IResult> ProxyToSearchServiceAsync(
    HttpContext httpContext,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
    ProxyAsync(httpContext, httpClientFactory, "http://localhost:5239", cancellationToken);

static Task<IResult> ProxyToTimelineServiceAsync(
    HttpContext httpContext,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
    ProxyAsync(httpContext, httpClientFactory, "http://localhost:5174", cancellationToken);

static Task<IResult> ProxyToPostsServiceAsync(
    HttpContext httpContext,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
    ProxyAsync(httpContext, httpClientFactory, "http://localhost:5237", cancellationToken);

static async Task<IResult> ProxyAsync(
    HttpContext context,
    IHttpClientFactory httpClientFactory,
    string destinationBaseUrl,
    CancellationToken cancellationToken)
{
    var targetUri = new Uri($"{destinationBaseUrl}{context.Request.Path}{context.Request.QueryString}");
    using var proxiedRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

    if (context.Request.ContentLength is > 0 || context.Request.Headers.ContainsKey("Transfer-Encoding"))
    {
        proxiedRequest.Content = new StreamContent(context.Request.Body);
    }

    foreach (var header in context.Request.Headers)
    {
        if (!proxiedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && proxiedRequest.Content is not null)
        {
            proxiedRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    proxiedRequest.Headers.Host = null;

    var httpClient = httpClientFactory.CreateClient();

    using var proxiedResponse = await httpClient.SendAsync(
        proxiedRequest,
        HttpCompletionOption.ResponseHeadersRead,
        cancellationToken);

    context.Response.StatusCode = (int)proxiedResponse.StatusCode;

    foreach (var header in proxiedResponse.Headers)
    {
        context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    foreach (var header in proxiedResponse.Content.Headers)
    {
        context.Response.Headers[header.Key] = header.Value.ToArray();
    }

    context.Response.Headers.Remove("transfer-encoding");

    await proxiedResponse.Content.CopyToAsync(context.Response.Body, cancellationToken);
    return Results.Empty;
}
