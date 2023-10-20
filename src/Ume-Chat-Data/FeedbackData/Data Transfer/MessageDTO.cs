using System.Text.Json.Serialization;

namespace Ume_Chat_Data_Feedback.Data_Transfer;

public class MessageDTO
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;

    [JsonPropertyName("citations")]
    public virtual ICollection<CitationDTO> Citations { get; set; } = new List<CitationDTO>();
}
