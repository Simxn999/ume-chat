using System.Xml.Serialization;

namespace Ume_Chat_Models.Ume_Chat_Data.Ume_Chat;

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
