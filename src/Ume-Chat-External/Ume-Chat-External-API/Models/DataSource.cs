using System.Text.Json.Serialization;

namespace Ume_Chat_External_API.Models;

public class DataSource
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("parameters")]
    public Parameters Parameters { get; set; } = default!;
}
