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