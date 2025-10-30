namespace GhaStatusProxy.Models;

public record GhaRunDto(
    string Repo,
    string Status,
    string? Conclusion,
    string Url,
    DateTimeOffset UpdatedAt,
    string? Name,
    string Event
);
