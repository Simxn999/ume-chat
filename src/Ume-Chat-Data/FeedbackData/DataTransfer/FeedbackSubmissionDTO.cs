using System.Text.Json.Serialization;

namespace FeedbackData.DataTransfer;

public class FeedbackSubmissionDTO
{
    [JsonPropertyName("messages")]
    public ICollection<MessageDTO> Messages { get; set; } = new List<MessageDTO>();

    [JsonPropertyName("categories")]
    public ICollection<int> CategoryIDs { get; set; } = new List<int>();

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}
