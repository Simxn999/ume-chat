using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Models.API.ChatAPI;

/// <summary>
///     Citation data transfer object
/// </summary>
[DebuggerDisplay("{Title}")]
public class ResponseCitation
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string URL { get; set; } = string.Empty;
}
