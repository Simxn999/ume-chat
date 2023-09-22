using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_External_General.Models.API.Response;

/// <summary>
///     API Response.
/// </summary>
[DebuggerDisplay("{Message} - {Citations?.Count ?? 0} Citations")]
public class ChatResponse
{
    /// <summary>
    ///     Response message aka chatbot answer.
    /// </summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    /// <summary>
    ///     Citations used by chatbot.
    /// </summary>
    [JsonPropertyName("citations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Citation>? Citations { get; set; }
}