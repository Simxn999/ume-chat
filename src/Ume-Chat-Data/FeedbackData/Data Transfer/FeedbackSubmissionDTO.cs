using System.Text.Json.Serialization;

namespace Ume_Chat_Data_Feedback.Data_Transfer;

public class FeedbackSubmissionDTO
{
    [JsonPropertyName("messages")]
    public ICollection<MessageDTO> Messages { get; set; } = new List<MessageDTO>();

    [JsonPropertyName("categories")]
    public ICollection<int> CategoryIDs { get; set; } = new List<int>();

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}
