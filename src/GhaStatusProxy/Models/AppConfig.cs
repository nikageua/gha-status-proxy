namespace GhaStatusProxy.Models;

public record AppConfig(
    string Token,
    string[] Repos
);
