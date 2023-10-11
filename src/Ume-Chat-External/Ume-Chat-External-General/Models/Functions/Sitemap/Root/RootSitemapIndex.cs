using System.Xml.Serialization;

namespace Ume_Chat_External_General.Models.Functions.Sitemap.Root;

/// <summary>
///     Root sitemap index from website containing root sitemap.
/// </summary>
public class RootSitemapIndex
{
    [XmlElement("sitemap")]
    public RootSitemap Sitemap { get; set; } = default!;
}
