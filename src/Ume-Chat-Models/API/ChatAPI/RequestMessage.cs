using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Models.API.ChatAPI;

/// <summary>
///     API Request body.
/// </summary>
[DebuggerDisplay("{Role}: {Content}")]
public class RequestMessage
{
    /// <summary>
    ///     Role of message.
    ///     Should be "user" or "assistant".
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    /// <summary>
    ///     Content of message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}
