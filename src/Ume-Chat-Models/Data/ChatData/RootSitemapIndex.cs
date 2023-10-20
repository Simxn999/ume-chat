using System.Xml.Serialization;

namespace Models.Data.ChatData;

/// <summary>
///     Root sitemap index from website containing root sitemap.
/// </summary>
public class RootSitemapIndex
{
    [XmlElement("sitemap")]
    public RootSitemap Sitemap { get; set; } = default!;
}
