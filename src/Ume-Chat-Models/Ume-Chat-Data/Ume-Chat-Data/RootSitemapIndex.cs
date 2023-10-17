using System.Xml.Serialization;

namespace Ume_Chat_Models.Ume_Chat_Data.Ume_Chat_Data;

/// <summary>
///     Root sitemap index from website containing root sitemap.
/// </summary>
public class RootSitemapIndex
{
    [XmlElement("sitemap")]
    public RootSitemap Sitemap { get; set; } = default!;
}
