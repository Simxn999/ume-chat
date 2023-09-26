using System.Diagnostics;
using System.Xml.Serialization;

namespace Ume_Chat_External_General.Models.Functions.Sitemap;

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
}
