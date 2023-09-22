using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;
using Ume_Chat_External_General.Models.Functions.Sitemap;

namespace Ume_Chat_External_Functions.Clients;

public class CrawlerClient
{
    private readonly ILogger _logger;

    private CrawlerClient(ILogger logger)
    {
        _logger = logger;
    }

    private IBrowser Browser { get; set; } = default!;
    private string TitleSuffix { get; set; } = default!;

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

    public async Task<IList<CrawledWebpage>> CrawlSitemapItemsAsync(IList<SitemapItem> sitemapItems)
    {
        _logger.LogInformation($"Crawling sitemap item{Grammar.GetPlurality(sitemapItems.Count, "", "s")}...");

        try
        {
            var output = new List<CrawledWebpage>();

            for (var i = 0; i < sitemapItems.Count; i++)
                output.Add(await CrawlSitemapItemAsync(sitemapItems[i], i + 1, sitemapItems.Count));

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

    private async Task InitializeAsync()
    {
        try
        {
            Browser = await GetBrowser();
            TitleSuffix = Variables.Get("CRAWLER_TITLE_SUFFIX");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed initialization of CrawlerClient!");
            throw;
        }
    }

    private async Task<IBrowser> GetBrowser()
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