using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_External_General.Models.API.Response;

[DebuggerDisplay("{Title}")]
public class ResponseCitation
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string URL { get; set; } = string.Empty;
}