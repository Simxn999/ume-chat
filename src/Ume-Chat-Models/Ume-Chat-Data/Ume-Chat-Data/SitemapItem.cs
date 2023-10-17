using System.Diagnostics;
using System.Xml.Serialization;

namespace Ume_Chat_Models.Ume_Chat_Data.Ume_Chat_Data;

/// <summary>
///     Sitemap items from sitemap.
/// </summary>
[DebuggerDisplay("{URL}")]
public class SitemapItem
{
    [XmlElement("loc")]
    public string URL { get; set; } = string.Empty;

    [XmlElement("lastmod")]
    public DateTimeOffset LastModified { get; set; }

    [XmlElement("priority")]
    public decimal Priority { get; set; }
}
