using System.Text.Json.Serialization;

namespace Ume_Chat_Models.Data.FeedbackData;

public class Citation
{
    [JsonIgnore]
    public Guid ID { get; } = Guid.NewGuid();

    [JsonPropertyName("document_id")]
    public Guid DocumentID { get; set; }

    [JsonPropertyName("text_id")]
    public string TextID { get; set; } = default!;

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonIgnore]
    public Guid MessageID { get; set; }

    [JsonIgnore]
    public virtual Message Message { get; set; } = default!;
}
