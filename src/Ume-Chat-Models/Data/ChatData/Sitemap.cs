using System.Diagnostics;
using System.Xml.Serialization;

namespace Models.Data.ChatData;

/// <summary>
///     Sitemap from website.
/// </summary>
public class Sitemap
{
    [XmlElement("url")]
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<SitemapItem> Items { get; set; } = default!;
}
