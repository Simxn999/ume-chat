using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Models.API.ChatAPI;

/// <summary>
///     Citation used by chatbot.
/// </summary>
/// <param name="documentNumber">Citation identifier - [docN]</param>
/// <param name="title">Title of citation</param>
/// <param name="url">URL of citation</param>
[DebuggerDisplay("{DocumentID} - {Title}")]
public class Citation(int documentNumber, string title, string url)
{
    /// <summary>
    ///     <para>Citation number based on the order that they appear in the text.</para>
    ///     <para> -1: Citation has not been numbered </para>
    ///     <para> >= 1: Occurrence number of citation in message </para>
    /// </summary>
    [JsonPropertyName("citationNumber")]
    public int CitationNumber { get; set; } = -1;

    /// <summary>
    ///     <para>Document identifier used by chatbot.</para>
    ///     <para>Example: [doc1], [doc2]</para>
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
