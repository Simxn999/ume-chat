using System.Diagnostics;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions.Sitemap;

namespace Ume_Chat_External_Functions.Clients;

[DebuggerDisplay("{SitemapURL}")]
public class SitemapClient(ILogger logger)
{
    private string SitemapURL { get; } = Variables.Get("SITEMAP_URL");
    private string SitemapNamespace { get; } = Variables.Get("SITEMAP_NAMESPACE");
    private string SitemapDownloadsURL { get; } = Variables.Get("SITEMAP_DOWNLOADS_URL");
    private string SitemapImagesURL { get; } = Variables.Get("SITEMAP_IMAGES_URL");

    public async Task<Sitemap> GetSitemapAsync()
    {
        logger.LogInformation("Retrieving sitemap...");

        try
        {
            using var httpClient = new HttpClient();
            var dataStream = await httpClient.GetStreamAsync(SitemapURL);

            await using var gzip = new GZipStream(dataStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            var xmlString = await reader.ReadToEndAsync();

            var xml = new XmlDocument();
            xml.LoadXml(xmlString);
            ArgumentNullException.ThrowIfNull(xml.DocumentElement, nameof(xml.DocumentElement));

            var serializer = new XmlSerializer(typeof(Sitemap), new XmlRootAttribute("urlset") { Namespace = SitemapNamespace });

            var sitemap = serializer.Deserialize(new XmlNodeReader(xml.DocumentElement)) as Sitemap;
            ArgumentNullException.ThrowIfNull(sitemap, nameof(sitemap));

            return sitemap;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed retrieval of sitemap!");
            throw;
        }
    }

    public List<SitemapItem> GetPages(Sitemap sitemap)
    {
        try
        {
            return sitemap.Items.Where(item => !item.URL.StartsWith(SitemapImagesURL) && !item.URL.StartsWith(SitemapDownloadsURL)).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed retrieval of pages!");
            throw;
        }
    }
}