using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_Models.Ume_Chat_API;

/// <summary>
///     API Request body.
/// </summary>
[DebuggerDisplay("{Role}: {Message}")]
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
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
