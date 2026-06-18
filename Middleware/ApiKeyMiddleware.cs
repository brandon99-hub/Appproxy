using System.Net;
using BIProxy.Models;
using Microsoft.Extensions.Options;

namespace BIProxy.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ProxySettings _proxySettings;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(
        RequestDelegate next,
        IOptions<ProxySettings> proxySettings,
        ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _proxySettings = proxySettings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
        {
            _logger.LogWarning("Missing X-Api-Key header");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API Key is missing.");
            return;
        }

        if (!_proxySettings.ApiKey.Equals(extractedApiKey))
        {
            _logger.LogWarning("Invalid API Key provided");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid API Key.");
            return;
        }

        await _next(context);
    }
}
