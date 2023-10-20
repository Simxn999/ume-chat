using System.Text.Json.Serialization;

namespace FeedbackData.DataTransfer;

public class CitationDTO
{
    [JsonPropertyName("document_id")]
    public Guid DocumentID { get; set; }

    [JsonPropertyName("text_id")]
    public string TextID { get; set; } = default!;

    [JsonPropertyName("position")]
    public int Position { get; set; }
}
