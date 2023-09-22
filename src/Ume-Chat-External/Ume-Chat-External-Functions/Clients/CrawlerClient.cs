using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;
using Ume_Chat_External_General.Models.Functions.Sitemap;

namespace Ume_Chat_External_Functions.Clients;

/// <summary>
///     Client for crawling and retrieving content of webpage.
/// </summary>
public class CrawlerClient
{
    private readonly ILogger _logger;

    private CrawlerClient(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Browser used for crawling the website.
    /// </summary>
    private IBrowser Browser { get; set; } = default!;

    /// <summary>
    ///     Default title suffix of webpages, irrelevant because they are the same for every webpage.
    /// </summary>
    private string TitleSuffix { get; set; } = default!;

    /// <summary>
    ///     Create CrawlerClient and initialize properties asynchronously.
    /// </summary>
    /// <param name="logger">ILogger</param>
    /// <returns>CrawlerClient</returns>
    public static async Task<CrawlerClient> CreateAsync(ILogger logger)
    {
        try
        {
            var crawler = new CrawlerClient(logger);
            await crawler.InitializeAsync();

            return crawler;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed creation of CrawlerClient!");
            throw;
        }
    }

    /// <summary>
    ///     Crawl and retrieve content of sitemap items.
    /// </summary>
    /// <param name="sitemapItems">Sitemap items to crawl</param>
    /// <returns>List of crawled webpages</returns>
    public async Task<IList<CrawledWebpage>> CrawlSitemapItemsAsync(IList<SitemapItem> sitemapItems)
    {
        _logger.LogInformation($"Crawling sitemap item{Grammar.GetPlurality(sitemapItems.Count, "", "s")}...");

        try
        {
            var output = new List<CrawledWebpage>();

            // Crawl every sitemap item and add it to the output list
            for (var i = 0; i < sitemapItems.Count; i++)
                output.Add(await CrawlSitemapItemAsync(sitemapItems[i], i + 1, sitemapItems.Count));

            // Remove webpages with invalid data
            output = output.Where(w => !string.IsNullOrEmpty(w.URL) && !string.IsNullOrEmpty(w.Title)
                                                                    && !string.IsNullOrEmpty(w.Content)).ToList();

            _logger.LogInformation($"Crawled {{count}} sitemap item{Grammar.GetPlurality(output.Count, "", "s")}!", output.Count);
            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed crawling of sitemap items!");
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
            Browser = await GetBrowserAsync();
            TitleSuffix = Variables.Get("CRAWLER_TITLE_SUFFIX");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed initialization of CrawlerClient!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the browser used for crawling.
    /// </summary>
    /// <returns>Browser used for crawling</returns>
    private async Task<IBrowser> GetBrowserAsync()
    {
        try
        {
            await new BrowserFetcher().DownloadAsync();
            Browser = await Puppeteer.LaunchAsync(new LaunchOptions { Timeout = 0, Args = new[] { "--no-sandbox" } });

            return Browser;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of Browser!");
            throw;
        }
    }

    /// <summary>
    ///     Crawl and retrieve content of sitemap item.
    /// </summary>
    /// <param name="sitemapItem">Sitemap item to crawl</param>
    /// <param name="index">Current index of sitemap item in batch</param>
    /// <param name="total">Total number of sitemap items in batch</param>
    /// <returns>Crawled webpage with content from webpage</returns>
    private async Task<CrawledWebpage> CrawlSitemapItemAsync(SitemapItem sitemapItem, int index, int total)
    {
        _logger.LogInformation($"{new ProgressString(index, total)} Crawling \"{{url}}\"...", sitemapItem.URL);

        try
        {
            await using var pageCrawler = await Browser.NewPageAsync();
            await pageCrawler.GoToAsync(sitemapItem.URL);

            var title = await RetrieveTitleOfPageAsync(pageCrawler);
            var content = await RetrieveContentOnPageAsync(pageCrawler);

            var output = new CrawledWebpage(sitemapItem.URL, title, content, sitemapItem.LastModified);

            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed crawling \"{url}\"!", sitemapItem.URL);
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the title of a webpage.
    /// </summary>
    /// <param name="page">PuppeteerSharp page to retrieve title from</param>
    /// <returns>Title of webpage</returns>
    private async Task<string> RetrieveTitleOfPageAsync(IPage page)
    {
        try
        {
            var title = (await page.GetTitleAsync()).Replace(TitleSuffix, "");

            if (string.IsNullOrEmpty(title))
                _logger.LogError("No title on \"{url}\"!", page.Url);

            return title;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of title!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the content of a webpage.
    /// </summary>
    /// <param name="page">PuppeteerSharp page to retrieve content from</param>
    /// <param name="elementTag">Optional: HTML element with desired content. Default: 'main'</param>
    /// <returns>Content of webpage</returns>
    private async Task<string> RetrieveContentOnPageAsync(IPage page, string? elementTag = "main")
    {
        try
        {
            const string function = "e => e?.innerText";

            var element = await page.QuerySelectorAsync(elementTag);
            var content = await page.EvaluateFunctionAsync<string>(function, element);

            if (string.IsNullOrEmpty(content) && elementTag != "body")
                content = await RetrieveContentOnPageAsync(page, "body");

            if (string.IsNullOrEmpty(content))
                _logger.LogError("No content on \"{url}\"!", page.Url);

            return content?.Trim() ?? string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of content!");
            throw;
        }
    }
}