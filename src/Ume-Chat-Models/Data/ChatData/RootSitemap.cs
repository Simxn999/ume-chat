using System.Xml.Serialization;

namespace Models.Data.ChatData;

/// <summary>
///     Root sitemap from website.
/// </summary>
public class RootSitemap
{
    [XmlElement("loc")]
    public string URL { get; set; } = string.Empty;

    [XmlElement("lastmod")]
    public DateTimeOffset LastModified { get; set; }
}
