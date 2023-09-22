using System.Diagnostics;
using System.Xml.Serialization;

namespace Ume_Chat_External_General.Models.Functions.Sitemap;

public class Sitemap
{
    [XmlElement("url")]
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<SitemapItem> Items { get; set; } = default!;
}
