using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_External_General.Models.API.Response;

[DebuggerDisplay("{DocumentID} - {Title}")]
public class Citation(int documentNumber, string title, string url)
{
    [JsonPropertyName("citationNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int CitationNumber { get; set; }

    [JsonPropertyName("documentID")]
    public string DocumentID { get; init; } = $"[doc{documentNumber}]";

    [JsonPropertyName("title")]
    public string Title { get; init; } = title;

    [JsonPropertyName("url")]
    public string URL { get; init; } = url;
}