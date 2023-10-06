using System.Text.Json.Serialization;

namespace Ume_Chat_External_API.Models;

public class Parameters
{
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = default!;

    [JsonPropertyName("key")]
    public string Key { get; set; } = default!;

    [JsonPropertyName("indexName")]
    public string IndexName { get; set; } = default!;

    [JsonPropertyName("queryType")]
    public string QueryType { get; set; } = default!;

    [JsonPropertyName("semanticConfiguration")]
    public string SemanticConfiguration { get; set; } = default!;

    [JsonPropertyName("embeddingEndpoint")]
    public string EmbeddingEndpoint { get; set; } = default!;

    [JsonPropertyName("embeddingKey")]
    public string EmbeddingKey { get; set; } = default!;

    [JsonPropertyName("roleInformation")]
    public string RoleInformation { get; set; } = default!;

    [JsonPropertyName("inScope")]
    public bool InScope { get; set; }

    [JsonPropertyName("fieldsMapping")]
    public FieldsMapping FieldsMapping { get; set; } = default!;
}
