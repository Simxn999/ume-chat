using System.Text.Json.Serialization;

namespace Models.Data.FeedbackData;

public class Category
{
    [JsonPropertyName("id")]
    public int ID { get; set; } = 1;

    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonIgnore]
    public virtual ICollection<FeedbackSubmission> FeedbackSubmissions { get; set; } = new List<FeedbackSubmission>();
}
