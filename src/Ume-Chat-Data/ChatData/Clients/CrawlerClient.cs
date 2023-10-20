using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Models.Data.ChatData;
using Utilities;

namespace Ume_Chat_Data.Clients;

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
    ///     Enumerable of titles that are irrelevant for the chatbot.
    /// </summary>
    private IEnumerable<string> ExcludedTitles { get; set; } = default!;

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
    public IList<CrawledWebpage> CrawlSitemapItems(IList<SitemapItem> sitemapItems)
    {
        _logger.LogInformation($"Crawling {{Count}} sitemap item{Grammar.GetPlurality(sitemapItems.Count, "", "s")}...", sitemapItems.Count);

        try
        {
            // Crawl every sitemap item synchronously
            var tasks = sitemapItems.Select(CrawlSitemapItemAsync).ToList();

            // Wait for every sitemap item to be crawled
            Task.WaitAll(tasks.Cast<Task>().ToArray());

            // Add the webpages to output
            var output = tasks.Select(task => task.Result).ToList();

            var invalidWebpages = GetInvalidWebpages(output);

            // Remove webpages with invalid data & return
            return output.Where(w => invalidWebpages.All(iw => iw.URL != w.URL)).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed crawling of sitemap item{Grammar.GetPlurality(sitemapItems.Count, "", "s")}!");
            throw;
        }
    }

    /// <summary>
    ///     Closes the browser instance used by the crawler.
    /// </summary>
    public async Task CloseBrowserAsync()
    {
        if (!Browser.IsClosed)
            await Browser.CloseAsync();
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
            ExcludedTitles = Variables.GetEnumerable("CRAWLER_EXCLUDED_TITLES").ToList();
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
            Browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Timeout = 0,
                Args = new[] { "--no-sandbox" }
            });

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
    /// <returns>Crawled webpage with content from webpage</returns>
    private async Task<CrawledWebpage> CrawlSitemapItemAsync(SitemapItem sitemapItem)
    {
        try
        {
            await using var pageCrawler = await Browser.NewPageAsync();
            await pageCrawler.GoToAsync(sitemapItem.URL);

            await ExpandElementsAsync(pageCrawler);
            var title = await RetrieveTitleAsync(pageCrawler);
            var content = await RetrieveContentAsync(pageCrawler);

            var output = new CrawledWebpage(sitemapItem.URL, title, content, sitemapItem.LastModified, sitemapItem.Priority);

            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed crawling \"{url}\"!", sitemapItem.URL);
            throw;
        }
    }

    /// <summary>
    ///     Expand all html elements that are expandable.
    /// </summary>
    /// <param name="page">PuppeteerSharp page to expand elements on</param>
    private async Task ExpandElementsAsync(IPage page)
    {
        try
        {
            const string script =
                """document.querySelectorAll('main [aria-expanded="false"]:not([aria-controls="sol-toolbar-box__share"])').forEach(b => b?.click())""";
            await page.EvaluateExpressionAsync(script);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed expanding elements!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the title of a webpage.
    /// </summary>
    /// <param name="page">PuppeteerSharp page to retrieve title from</param>
    /// <returns>Title of webpage</returns>
    private async Task<string> RetrieveTitleAsync(IPage page)
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
    private async Task<string> RetrieveContentAsync(IPage page, string? elementTag = "main")
    {
        try
        {
            const string function = "e => e?.innerText";

            var element = await page.QuerySelectorAsync(elementTag);
            var content = await page.EvaluateFunctionAsync<string>(function, element);

            if (string.IsNullOrEmpty(content) && elementTag != "body")
                content = await RetrieveContentAsync(page, "body");

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

    /// <summary>
    ///     Retrieve webpages that are empty or should be excluded based on ExcludedTitles.
    /// </summary>
    /// <param name="webpages">List of webpages to validate</param>
    /// <returns>List of valid webpages</returns>
    private List<CrawledWebpage> GetInvalidWebpages(IEnumerable<CrawledWebpage> webpages)
    {
        var invalidWebpages = webpages
                             .Where(w => string.IsNullOrEmpty(w.Title) || string.IsNullOrEmpty(w.Content) || ExcludedTitles.Any(t => t.Equals(w.Title)))
                             .ToList();

        if (invalidWebpages.Count > 0)
            _logger.LogInformation($"{{Count}} sitemap item{Grammar.GetPlurality(invalidWebpages.Count, "", "s")} {Grammar.GetPlurality(invalidWebpages.Count, "was", "were")} determined invalid!",
                                   invalidWebpages.Count);

        for (var i = 0; i < invalidWebpages.Count; i++)
            _logger.LogInformation($"Invalid sitemap item #{i + 1}: {{URL}}", invalidWebpages[i].URL);

        return invalidWebpages;
    }
}
