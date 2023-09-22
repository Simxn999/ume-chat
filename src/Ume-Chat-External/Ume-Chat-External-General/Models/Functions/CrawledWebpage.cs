using System.Diagnostics;

namespace Ume_Chat_External_General.Models.Functions;

[DebuggerDisplay("{Title} - {URL}")]
public class CrawledWebpage(string url, string title, string content, DateTimeOffset lastModified)
{
    public string URL { get; } = url;
    public string Title { get; } = title;
    public string Content { get; } = content;
    public DateTimeOffset LastModified { get; } = lastModified;
}