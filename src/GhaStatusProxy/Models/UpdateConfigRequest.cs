using System.Text.Json.Serialization;

namespace GhaStatusProxy.Models;

public class UpdateConfigRequest
{

    #region Properties: Public

    [JsonPropertyName("token")] public string? Token { get; set; }
    [JsonPropertyName("repos")] public string[]? Repos { get; set; }

    #endregion
}
