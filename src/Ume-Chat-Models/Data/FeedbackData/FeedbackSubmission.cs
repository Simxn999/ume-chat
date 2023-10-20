using System.Text.Json.Serialization;

namespace Ume_Chat_Models.Data.FeedbackData;

public class FeedbackSubmission
{
    [JsonPropertyName("id")]
    public Guid ID { get; } = Guid.NewGuid();

    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("messages")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [JsonIgnore]
    [JsonPropertyName("status_id")]
    public int StatusID { get; set; }

    [JsonPropertyName("status")]
    public virtual Status Status { get; set; } = default!;

    [JsonPropertyName("categories")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
