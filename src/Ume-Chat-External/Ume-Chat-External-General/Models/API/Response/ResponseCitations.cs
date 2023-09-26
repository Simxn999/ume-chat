using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_External_General.Models.API.Response;

/// <summary>
///     Citations data transfer object
/// </summary>
public class ResponseCitations
{
    [JsonPropertyName("citations")]
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<ResponseCitation> Citations { get; set; } = default!;
}
