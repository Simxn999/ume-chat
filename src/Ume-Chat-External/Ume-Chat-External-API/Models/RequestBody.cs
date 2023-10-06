using System.Text.Json.Serialization;
using Ume_Chat_External_General.Models.API.Request;

namespace Ume_Chat_External_API.Models;

public class RequestBody
{
    [JsonPropertyName("messages")]
    public IEnumerable<RequestMessage> Messages { get; set; } = default!;

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = default!;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("dataSources")]
    public IEnumerable<DataSource> DataSources { get; set; } = default!;
}
