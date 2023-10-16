using System.Diagnostics;
using System.Xml.Serialization;

namespace Ume_Chat_Models.Ume_Chat_Data.Ume_Chat;

/// <summary>
///     Sitemap from website.
/// </summary>
public class Sitemap
{
    [XmlElement("url")]
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<SitemapItem> Items { get; set; } = default!;
}
