using System.Text.Json.Serialization;

namespace Ume_Chat_Models.Data.FeedbackData;

public class Message
{
    [JsonIgnore]
    public Guid ID { get; } = Guid.NewGuid();

    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;

    [JsonIgnore]
    public int Position { get; set; }

    [JsonPropertyName("citations")]
    public virtual ICollection<Citation> Citations { get; set; } = new List<Citation>();

    [JsonIgnore]
    public Guid FeedbackSubmissionID { get; set; }

    [JsonIgnore]
    public virtual FeedbackSubmission FeedbackSubmission { get; set; } = default!;
}
