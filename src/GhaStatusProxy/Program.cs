using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Endpoints;
using GhaStatusProxy.Options;
using GhaStatusProxy.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<ConfigStoreOptions>(opts =>
{
    opts.Path = Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "config.json";
});
builder.Services.AddSingleton<IConfigStore>(sp =>
{
    var fileOptions = sp.GetRequiredService<IOptions<ConfigStoreOptions>>();
    var logger = sp.GetRequiredService<ILogger<EnvConfigStore>>();
    var env = new EnvConfigStore(logger);
    var file = new FileConfigStore(fileOptions);
    return new CompositeConfigStore(env, file);
});
builder.Services.AddSingleton<IGitHubClientFactory, GitHubClientFactory>();
builder.Services.AddSingleton<IGhaService, GhaService>();

var app = builder.Build();

app.MapConfigEndpoints();
app.MapGhaEndpoints();

app.Run();

public partial class Program { }
