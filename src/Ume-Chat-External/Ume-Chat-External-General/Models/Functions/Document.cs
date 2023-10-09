using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ume_Chat_External_General.Models.Functions;

/// <summary>
///     Document that is uploaded to the database/index.
/// </summary>
[DebuggerDisplay("{ChunkID}-{Title} - {URL}")]
public class Document
{
    /// <summary>
    ///     GUID as ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     URL of document.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? URL { get; set; }

    /// <summary>
    ///     Title of document.
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>
    ///     Path of document.
    /// </summary>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }

    /// <summary>
    ///     Content of document.
    /// </summary>
    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }

    /// <summary>
    ///     Embedding based on Content.
    /// </summary>
    [JsonPropertyName("vector")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<float>? Vector { get; set; }

    /// <summary>
    ///     Keywords based on Title.
    /// </summary>
    [JsonPropertyName("keywords_title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? KeywordsTitle { get; set; }

    /// <summary>
    ///     Keywords based on Path.
    /// </summary>
    [JsonPropertyName("keywords_path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? KeywordsPath { get; set; }

    /// <summary>
    ///     Keywords based on Content.
    /// </summary>
    [JsonPropertyName("keywords_content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? KeywordsContent { get; set; }

    /// <summary>
    ///     Groups who has access to the current document.
    /// </summary>
    [JsonPropertyName("group_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? GroupIDs { get; set; }

    /// <summary>
    ///     Date of the last time the webpage was updated.
    /// </summary>
    [JsonPropertyName("lastmod")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? LastModified { get; set; }

    /// <summary>
    ///     Identifier of chunk
    /// </summary>
    [JsonPropertyName("chunk_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ChunkID { get; set; }
}
