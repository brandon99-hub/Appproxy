using BIProxy.Middleware;
using BIProxy.Models;
using BIProxy.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    var port = builder.Configuration.GetValue<int>("ProxySettings:Port");
    options.ListenAnyIP(port);
});

builder.Services.AddControllers();

builder.Services.Configure<BCSettings>(builder.Configuration.GetSection("BCSettings"));
builder.Services.Configure<ProxySettings>(builder.Configuration.GetSection("ProxySettings"));

builder.Services.AddHttpClient<BCProxyService>(client =>
{
    var baseUrl = builder.Configuration["BCSettings:BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl))
    {
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromMinutes(5);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseDefaultCredentials = true,
    PreAuthenticate = true
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var port = builder.Configuration.GetValue<int>("ProxySettings:Port");
logger.LogInformation("Admissions Proxy starting on port {Port}", port);
logger.LogInformation("Forwarding to BC: {BaseUrl}", builder.Configuration.GetValue<string>("BCSettings:BaseUrl"));

app.Run();
