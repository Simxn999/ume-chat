using System.Text.Json.Serialization;

namespace FeedbackData.DataTransfer;

public class MessageDTO
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;

    [JsonPropertyName("citations")]
    public virtual ICollection<CitationDTO> Citations { get; set; } = new List<CitationDTO>();
}
