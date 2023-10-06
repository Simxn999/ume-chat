using System.Text.Json.Serialization;

namespace Ume_Chat_External_API.Models;

public class FieldsMapping
{
    [JsonPropertyName("titleField")]
    public string TitleField { get; set; } = default!;

    [JsonPropertyName("urlField")]
    public string URLField { get; set; } = default!;

    [JsonPropertyName("contentFields")]
    public IEnumerable<string> ContentFields { get; set; } = default!;

    [JsonPropertyName("vectorFields")]
    public IEnumerable<string> VectorFields { get; set; } = default!;
}
