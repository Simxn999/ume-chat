using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_Models.Ume_Chat_API;

/// <summary>
///     Citations data transfer object
/// </summary>
public class ResponseCitations
{
    [JsonPropertyName("citations")]
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<ResponseCitation> Citations { get; set; } = default!;
}
