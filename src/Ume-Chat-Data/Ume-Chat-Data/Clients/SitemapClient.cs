using System.Diagnostics;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Ume_Chat_Models.Ume_Chat_Data.Ume_Chat;
using Ume_Chat_Utilities;

namespace Ume_Chat_Data.Clients;

/// <summary>
///     Client for handling sitemap.
/// </summary>
[DebuggerDisplay("{RootURL}")]
public class SitemapClient
{
    private readonly ILogger _logger;

    private SitemapClient(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Last synchronized time.
    /// </summary>
    public DateTimeOffset LastModified { get; set; }

    /// <summary>
    ///     URL to root sitemap.
    /// </summary>
    private string RootURL { get; set; } = string.Empty;

    /// <summary>
    ///     Namespace of sitemap.
    /// </summary>
    private string SitemapNamespace { get; set; } = string.Empty;

    /// <summary>
    ///     URL to sitemap.
    /// </summary>
    private string URL { get; set; } = string.Empty;

    /// <summary>
    ///     Enumerable of segments in URLs that should be excluded from the database.
    /// </summary>
    private IEnumerable<string> SitemapExcludedURLSegments { get; } = Variables.GetEnumerable("SITEMAP_EXCLUDED_URL_SEGMENTS").ToList();

    /// <summary>
    ///     Create SitemapClient and initialize properties asynchronously.
    /// </summary>
    /// <param name="logger">ILogger</param>
    /// <returns>SitemapClient</returns>
    public static async Task<SitemapClient> CreateAsync(ILogger logger)
    {
        try
        {
            var sitemapClient = new SitemapClient(logger);
            await sitemapClient.InitializeAsync();

            return sitemapClient;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed creating SitemapClient!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve sitemap for website.
    /// </summary>
    /// <returns>Sitemap for website</returns>
    public async Task<Sitemap> GetSitemapAsync()
    {
        _logger.LogInformation("Retrieving sitemap...");

        try
        {
            using var httpClient = new HttpClient();
            var dataStream = await httpClient.GetStreamAsync(URL);

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
            _logger.LogError(e, "Failed retrieval of sitemap!");
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
            return sitemap.Items.Where(item => !SitemapExcludedURLSegments.Any(k => item.URL.Contains(k))).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of pages!");
            throw;
        }
    }

    /// <summary>
    ///     Initialize properties asynchronously.
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            RootURL = Variables.Get("SITEMAP_URL");
            SitemapNamespace = Variables.Get("SITEMAP_NAMESPACE");
            var rootSitemap = await GetRootSitemapAsync();

            URL = rootSitemap.URL;
            LastModified = rootSitemap.LastModified;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed initialization of SitemapClient!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve root sitemap for website.
    /// </summary>
    /// <returns>RootSitemap for website</returns>
    private async Task<RootSitemap> GetRootSitemapAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            var xmlString = await httpClient.GetStringAsync(RootURL);

            var xml = new XmlDocument();
            xml.LoadXml(xmlString);
            ArgumentNullException.ThrowIfNull(xml.DocumentElement, nameof(xml.DocumentElement));

            // Retrieve sitemap from xml
            var serializer = new XmlSerializer(typeof(RootSitemapIndex), new XmlRootAttribute("sitemapindex") { Namespace = SitemapNamespace });
            var rootSitemap = serializer.Deserialize(new XmlNodeReader(xml.DocumentElement)) as RootSitemapIndex;
            ArgumentNullException.ThrowIfNull(rootSitemap, nameof(rootSitemap));

            return rootSitemap.Sitemap;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of root sitemap!");
            throw;
        }
    }
}
