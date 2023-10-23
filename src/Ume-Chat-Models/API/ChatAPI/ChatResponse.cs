using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Models.API.ChatAPI;

/// <summary>
///     API Response.
/// </summary>
[DebuggerDisplay("{Content} - {Citations?.Count ?? 0} Citations")]
public class ChatResponse
{
    /// <summary>
    ///     Response message content aka chatbot answer.
    /// </summary>
    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }

    /// <summary>
    ///     Citations used by chatbot.
    /// </summary>
    [JsonPropertyName("citations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Citation>? Citations { get; set; }
}
