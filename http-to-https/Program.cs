using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Kestrel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

var port = ushort.Parse(Environment.GetEnvironmentVariable("PORT") ?? "8080");

await Host.Create(appHook: app => app.Run(RedirectToHttpsAsync))
    .Handler(new NotFoundHandler())
    .Port(port)
    .RunAsync();
return;

static Task RedirectToHttpsAsync(HttpContext context)
{
    var request = context.Request;
    var host = request.Host;

    if (string.IsNullOrEmpty(host.Host))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return Task.CompletedTask;
    }

    var rawTarget = context.Features.Get<IHttpRequestFeature>()?.RawTarget;

    if (string.IsNullOrEmpty(rawTarget) || rawTarget[0] != '/')
        rawTarget = string.Concat(
            request.PathBase.ToUriComponent(),
            request.Path.ToUriComponent(),
            request.QueryString.ToUriComponent());

    var authority = host.Port is null or 80
        ? new HostString(host.Host).ToUriComponent()
        : host.ToUriComponent();

    context.Response.StatusCode = StatusCodes.Status308PermanentRedirect;
    context.Response.Headers.Location = string.Concat("https://", authority, rawTarget);

    return Task.CompletedTask;
}

internal sealed class NotFoundHandler : IHandler
{
    public ValueTask PrepareAsync(IServer server)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        return new ValueTask<IResponse?>(request.Respond()
            .Status(ResponseStatus.NotFound)
            .Build());
    }
}