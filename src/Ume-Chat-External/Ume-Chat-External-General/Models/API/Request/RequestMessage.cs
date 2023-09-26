using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_External_General.Models.API.Request;

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
