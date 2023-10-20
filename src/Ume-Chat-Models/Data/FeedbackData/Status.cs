using System.Text.Json.Serialization;

namespace Models.Data.FeedbackData;

public class Status
{
    [JsonPropertyName("id")]
    public int ID { get; set; } = 1;

    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonIgnore]
    public virtual ICollection<FeedbackSubmission> FeedbackSubmissions { get; set; } = new List<FeedbackSubmission>();
}
