using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Endpoints;
using GhaStatusProxy.Options;
using GhaStatusProxy.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConfigStoreOptions>(opts =>
{
    opts.Path = Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "config.json";
});

builder.Services.AddSingleton<IConfigStore, FileConfigStore>();
builder.Services.AddSingleton<IGitHubClientFactory, GitHubClientFactory>();
builder.Services.AddSingleton<IGhaService, GhaService>();

var app = builder.Build();

app.MapConfigEndpoints();
app.MapGhaEndpoints();

app.Run();

public partial class Program { }
