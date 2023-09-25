using System.Diagnostics;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions.Sitemap;

namespace Ume_Chat_External_Functions.Clients;

/// <summary>
///     Client for handling sitemap.
/// </summary>
/// <param name="logger">ILogger</param>
[DebuggerDisplay("{SitemapURL}")]
public class SitemapClient(ILogger logger)
{
    /// <summary>
    ///     URL to sitemap.
    /// </summary>
    private string SitemapURL { get; } = Variables.Get("SITEMAP_URL");

    /// <summary>
    ///     Namespace of sitemap.
    /// </summary>
    private string SitemapNamespace { get; } = Variables.Get("SITEMAP_NAMESPACE");

    /// <summary>
    ///     Enumerable of URLs that are irrelevant for the chatbot.
    /// </summary>
    private IEnumerable<string> SitemapExcludedURLs { get; } = Variables.GetEnumerable("SITEMAP_EXCLUDED_URLS").ToList();

    /// <summary>
    ///     Retrieve sitemap for website.
    /// </summary>
    /// <returns>Sitemap for website</returns>
    public async Task<Sitemap> GetSitemapAsync()
    {
        logger.LogInformation("Retrieving sitemap...");

        try
        {
            using var httpClient = new HttpClient();
            var dataStream = await httpClient.GetStreamAsync(SitemapURL);

            // Decompress gzip
            await using var gzip = new GZipStream(dataStream, CompressionMode.Decompress);

            // Convert gzip to xml
            using var reader = new StreamReader(gzip);
            var xmlString = await reader.ReadToEndAsync();
            var xml = new XmlDocument();
            xml.LoadXml(xmlString);
            ArgumentNullException.ThrowIfNull(xml.DocumentElement, nameof(xml.DocumentElement));

            // Retrieve sitemap from xml
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

    /// <summary>
    ///     Retrieve relevant items from sitemap.
    /// </summary>
    /// <param name="sitemap">Sitemap to handle</param>
    /// <returns>List of sitemap items</returns>
    public List<SitemapItem> GetPages(Sitemap sitemap)
    {
        try
        {
            return sitemap.Items.Where(item => !SitemapExcludedURLs.Any(u => item.URL.StartsWith(u))).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed retrieval of pages!");
            throw;
        }
    }
}