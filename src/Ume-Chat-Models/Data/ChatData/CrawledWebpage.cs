using System.Diagnostics;

namespace Models.Data.ChatData;

/// <summary>
///     Crawled webpage content.
/// </summary>
/// <param name="url">URL of webpage</param>
/// <param name="title">Title of webpage</param>
/// <param name="content">Content of webpage</param>
/// <param name="lastModified">Last modified date of webpage</param>
/// <param name="priority">Priority weight of webpage</param>
[DebuggerDisplay("{Title} - {URL}")]
public class CrawledWebpage(string url, string title, string content, DateTimeOffset lastModified, decimal priority)
{
    public string URL { get; } = url;
    public string Title { get; } = title;
    public string Content { get; } = content;
    public DateTimeOffset LastModified { get; } = lastModified;
    public decimal Priority { get; } = priority;
}
