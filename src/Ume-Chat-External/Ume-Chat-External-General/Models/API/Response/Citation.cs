using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_External_General.Models.API.Response;

/// <summary>
///     Citation used by chatbot.
/// </summary>
/// <param name="documentNumber">Citation identifier - [docX]</param>
/// <param name="title">Title of citation</param>
/// <param name="url">URL of citation</param>
[DebuggerDisplay("{DocumentID} - {Title}")]
public class Citation(int documentNumber, string title, string url)
{
    /// <summary>
    ///     Citation number based on the order that they appear in the text.
    /// </summary>
    [JsonPropertyName("citationNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int CitationNumber { get; set; }

    /// <summary>
    ///     Document identifier used by chatbot.
    ///     Example: [doc1], [doc2]
    /// </summary>
    [JsonPropertyName("documentID")]
    public string DocumentID { get; init; } = $"[doc{documentNumber}]";

    /// <summary>
    ///     Title of citation.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = title;

    /// <summary>
    ///     URL of citation.
    /// </summary>
    [JsonPropertyName("url")]
    public string URL { get; init; } = url;
}