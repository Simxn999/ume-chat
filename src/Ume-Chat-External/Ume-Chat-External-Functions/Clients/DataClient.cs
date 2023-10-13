using System.Diagnostics;
using System.Globalization;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;
using Ume_Chat_External_General.Models.Functions.Sitemap;

namespace Ume_Chat_External_Functions.Clients;

/// <summary>
///     Client for synchronizing data between website & database.
/// </summary>
[DebuggerDisplay("{IndexClient.Index}")]
public class DataClient
{
    private readonly ILogger _logger;

    private DataClient(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Whether or not the database is already synchronized.
    /// </summary>
    private bool IsSynchronized { get; set; }

    /// <summary>
    ///     Client for handling sitemap.
    /// </summary>
    private SitemapClient SitemapClient { get; set; } = default!;

    /// <summary>
    ///     Client for handling database/index.
    /// </summary>
    private IndexClient IndexClient { get; set; } = default!;

    /// <summary>
    ///     List of documents inside of the database/index.
    /// </summary>
    private List<Document> Index { get; set; } = default!;

    /// <summary>
    ///     Sitemap of website.
    /// </summary>
    private Sitemap Sitemap { get; set; } = default!;

    /// <summary>
    ///     Relevant items from the sitemap.
    /// </summary>
    private List<SitemapItem> SitemapItems { get; set; } = default!;

    /// <summary>
    ///     Client for chunking webpage content.
    /// </summary>
    private ChunkerClient ChunkerClient { get; set; } = default!;

    /// <summary>
    ///     Client for crawling webpages.
    /// </summary>
    private CrawlerClient CrawlerClient { get; set; } = default!;

    /// <summary>
    ///     Client for handling embeddings.
    /// </summary>
    private EmbeddingsClient EmbeddingsClient { get; set; } = default!;

    /// <summary>
    ///     Client for handling keywords.
    /// </summary>
    private KeywordsClient KeywordsClient { get; set; } = default!;

    /// <summary>
    ///     Default groups used on documents.
    /// </summary>
    private IEnumerable<string> DefaultGroups { get; set; } = default!;

    /// <summary>
    ///     Size of batches.
    /// </summary>
    private int BatchSize { get; set; }

    /// <summary>
    ///     Create DataClient and initialize properties asynchronously.
    /// </summary>
    /// <param name="logger">ILogger</param>
    /// <returns>DataClient</returns>
    public static async Task<DataClient> CreateAsync(ILogger logger)
    {
        try
        {
            var dataFactory = new DataClient(logger);
            await dataFactory.InitializeAsync();

            return dataFactory;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed creating DataClient!");
            throw;
        }
    }

    /// <summary>
    ///     Synchronize data between website & database.
    /// </summary>
    public async Task SynchronizeAsync()
    {
        if (IsSynchronized)
        {
            _logger.LogInformation("Database is already synchronized with website!");
            return;
        }

        _logger.LogInformation("Synchronizing database with sitemap...");
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var invalidDocuments = GetInvalidDocuments();
            if (invalidDocuments.Count > 0)
                await DeleteDocumentsAsync(invalidDocuments);

            var sitemapItemsToUpdate = GetSitemapItemsToUpdate();

            if (sitemapItemsToUpdate.Count > 0)
            {
                var pageBatches = Batch(sitemapItemsToUpdate);
                await RunBatchesAsync(pageBatches);
            }

            stopwatch.Stop();
            _logger.LogInformation($"Synchronization successfull! {Math.Round(stopwatch.Elapsed.TotalSeconds, 2).ToString(CultureInfo.InvariantCulture)}s");

            Variables.Set("LAST_SYNCHRONIZED", SitemapClient.LastModified.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        }
        catch (Exception e)
        {
            stopwatch.Stop();
            _logger.LogError(e,
                             $"Synchronization failed! {Math.Round(stopwatch.Elapsed.TotalSeconds, 2).ToString(CultureInfo.InvariantCulture)}s");
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
            SitemapClient = await SitemapClient.CreateAsync(_logger);

            IsSynchronized = Variables.GetDateTimeOffset("LAST_SYNCHRONIZED") == SitemapClient.LastModified;
            if (IsSynchronized)
                return;

            IndexClient = new IndexClient(_logger);
            CrawlerClient = await CrawlerClient.CreateAsync(_logger);
            ChunkerClient = new ChunkerClient(_logger);
            EmbeddingsClient = new EmbeddingsClient(_logger);
            KeywordsClient = new KeywordsClient(_logger);
            DefaultGroups = Variables.GetEnumerable("DOCUMENT_DEFAULT_GROUPS");
            Index = await IndexClient.GetDocumentsForComparisonAsync();
            Sitemap = await SitemapClient.GetSitemapAsync();
            SitemapItems = SitemapClient.GetPages(Sitemap);
            BatchSize = Variables.GetInt("DATA_SYNCHRONIZATION_BATCH_SIZE");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed initialization of DataClient!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the sitemap items that are outdated in the database.
    /// </summary>
    /// <returns>List of sitemap items to update</returns>
    private List<SitemapItem> GetSitemapItemsToUpdate()
    {
        _logger.LogInformation("Retrieving items from sitemap to update...");
        try
        {
            return SitemapItems.Where(i =>
                                      {
                                          // Retrieve document from database
                                          var document = Index.FirstOrDefault(d => d.URL == i.URL);

                                          // Sitemap item should be updated if:
                                          //    Document.URL does not exist in database
                                          //        OR
                                          //    Webpage has been updated since it was uploaded to database
                                          return document is null || document.LastModified < i.LastModified;
                                      })
                               .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of sitemap items to update!");
            throw;
        }
    }

    /// <summary>
    ///     Split sitemap itemes into batches of [BatchSize] size.
    /// </summary>
    /// <param name="list">Enumerable to batch</param>
    /// <returns>List of lists of sitemap items</returns>
    private List<List<SitemapItem>> Batch(ICollection<SitemapItem> list)
    {
        _logger.LogInformation($"Batching {{Count}} sitemap item{Grammar.GetPlurality(list.Count, "", "s")}...",
                               list.Count);

        try
        {
            return list.Chunk(BatchSize).Select(b => b.ToList()).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed batching of sitemap items!");
            throw;
        }
    }

    /// <summary>
    ///     Execute batches.
    /// </summary>
    /// <param name="batches">Batches/List of lists of sitemap items to execute</param>
    private async Task RunBatchesAsync(List<List<SitemapItem>> batches)
    {
        try
        {
            for (var i = 0; i < batches.Count; i++)
                await RunBatchAsync(batches[i], i + 1, batches.Count);

            await CrawlerClient.CloseBrowserAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed running batches!");
            throw;
        }
    }

    /// <summary>
    ///     Execute batch.
    /// </summary>
    /// <param name="batch">Batch/List of sitemap items to execute.</param>
    /// <param name="index">Current index of batch in batches</param>
    /// <param name="total">Total number of batches</param>
    private async Task RunBatchAsync(IList<SitemapItem> batch, int index, int total)
    {
        _logger.LogInformation($"{new ProgressString(index, total)} Running batch...");

        try
        {
            if (batch.Count == 0)
                return;

            // Crawling
            var webpages = CrawlerClient.CrawlSitemapItems(batch).ToList();

            if (webpages.Count == 0)
                return;

            // Chunking
            var documents = ChunkerClient.ChunkCrawledWebpages(webpages);

            if (documents.Count == 0)
                return;

            // Embeddings
            EmbeddingsClient.PopulateDocumentsWithEmbeddings(ref documents);

            // Keywords
            KeywordsClient.PopulateDocumentsWithKeywords(ref documents);

            // Groups
            PopulateDocumentsWithDefaultGroups(ref documents);

            // Upload
            await UploadDocumentsAsync(documents);

            // Delete
            var documentsToDelete = GetOutdatedDocumentsFromBatch(batch);

            if (documentsToDelete.Count == 0)
                return;

            await DeleteDocumentsAsync(documentsToDelete);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed batch {index}!", index);
            throw;
        }
    }

    /// <summary>
    ///     Populates documents with default group ids.
    /// </summary>
    /// <param name="documents">Reference to list of documents to populate with groups</param>
    private void PopulateDocumentsWithDefaultGroups(ref List<Document> documents)
    {
        try
        {
            _logger.LogInformation($"Populating {{Count}} document{Grammar.GetPlurality(documents.Count, "", "s")} with default groups...",
                                   documents.Count);

            documents.ForEach(d => d.GroupIDs = DefaultGroups);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed populating documents with default groups!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve documents from database that should be deleted because of update.
    /// </summary>
    /// <param name="batch">Batch to filter</param>
    /// <returns>List of outdated documents</returns>
    private List<Document> GetOutdatedDocumentsFromBatch(ICollection<SitemapItem> batch)
    {
        _logger.LogInformation("Retrieving outdated documents from batch...");
        try
        {
            return Index.Where(d => batch.Any(item => item.URL == d.URL)).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of outdated documents!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve documents that don't exist in the sitemap.
    ///     In other words, documents with webpages that no longer exist.
    /// </summary>
    /// <returns>List of documents that should no longer exist</returns>
    private List<Document> GetInvalidDocuments()
    {
        _logger.LogInformation("Retrieving invalid documents...");
        try
        {
            return Index.Where(d => SitemapItems.All(item => d.URL != item.URL)).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of invalid documents!");
            throw;
        }
    }

    /// <summary>
    ///     Delete documents from database.
    /// </summary>
    /// <param name="documents">Documents to delete</param>
    private async Task DeleteDocumentsAsync(ICollection<Document> documents)
    {
        _logger.LogInformation($"Deleting {{count}} document{Grammar.GetPlurality(documents.Count, "", "s")}...",
                               documents.Count);

        try
        {
            if (documents.Count == 0)
                return;

            await IndexClient.IndexDocumentsAsync(documents, IndexDocumentsAction.Delete);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed deletion of documents!");
            throw;
        }
    }

    /// <summary>
    ///     Upload documents to database.
    /// </summary>
    /// <param name="documents">Documents to upload</param>
    private async Task UploadDocumentsAsync(ICollection<Document> documents)
    {
        _logger.LogInformation($"Uploading {{count}} document{Grammar.GetPlurality(documents.Count, "", "s")}...",
                               documents.Count);

        try
        {
            if (documents.Count == 0)
                return;

            await IndexClient.IndexDocumentsAsync(documents, IndexDocumentsAction.MergeOrUpload);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed uploading documents!");
            throw;
        }
    }
}
