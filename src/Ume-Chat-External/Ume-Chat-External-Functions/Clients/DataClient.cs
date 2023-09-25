using System.Diagnostics;
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
    private OpenAIEmbeddingsClient OpenAIEmbeddingsClient { get; set; } = default!;

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
        _logger.LogInformation("Synchronizing database with sitemap...");

        try
        {
            var invalidDocuments = GetInvalidDocuments();
            if (invalidDocuments.Count > 0)
                await DeleteDocumentsAsync(invalidDocuments);

            var sitemapItemsToUpdate = GetSitemapItemsToUpdate();

            if (sitemapItemsToUpdate.Count > 0)
            {
                var pageBatches = Batch(sitemapItemsToUpdate);
                await RunBatches(pageBatches);
            }

            _logger.LogInformation("Synchronization successfull!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Synchronization failed!");
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
            var sitemapper = new SitemapClient(_logger);
            IndexClient = new IndexClient(_logger);
            CrawlerClient = await CrawlerClient.CreateAsync(_logger);
            ChunkerClient = new ChunkerClient(_logger);
            OpenAIEmbeddingsClient = new OpenAIEmbeddingsClient(_logger);
            Index = await IndexClient.GetDocumentsForComparisonAsync();
            Sitemap = await sitemapper.GetSitemapAsync();
            SitemapItems = sitemapper.GetPages(Sitemap);
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
            var sitemapItems = SitemapItems.Where(i =>
                                                  {
                                                      // Retrieve document from database
                                                      var document = Index.FirstOrDefault(d => d.URL == i.URL);

                                                      // Sitemap item should be updated if:
                                                      //    Document.URL does not exist in database
                                                      //        OR
                                                      //    Webpage has been updated since it was uploaded to database
                                                      return document is null || document.LastModified < i.LastModified;
                                                  }).ToList();

            _logger.LogInformation($"Retrieved {{count}} item{Grammar.GetPlurality(sitemapItems.Count, "", "s")} from sitemap!", sitemapItems.Count);
            return sitemapItems;
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
    private List<List<SitemapItem>> Batch(IEnumerable<SitemapItem> list)
    {
        _logger.LogInformation("Batching sitemap items...");

        try
        {
            var batches = list.Chunk(BatchSize).Select(b => b.ToList()).ToList();

            return batches;
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
    private async Task RunBatches(List<List<SitemapItem>> batches)
    {
        _logger.LogInformation($"Running {{count}} batch{Grammar.GetPlurality(batches.Count, "", "es")}...", batches.Count);

        try
        {
            for (var i = 0; i < batches.Count; i++)
                await RunBatch(batches[i], i + 1, batches.Count);

            _logger.LogInformation($"Successfully ran {{count}} batch{Grammar.GetPlurality(batches.Count, "", "es")}!", batches.Count);
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
    private async Task RunBatch(IList<SitemapItem> batch, int index, int total)
    {
        _logger.LogInformation($"{new ProgressString(index, total)} Running batch...");

        try
        {
            // Crawling
            var webpages = (await CrawlerClient.CrawlSitemapItemsAsync(batch)).ToList();

            // Chunking
            var documents = ChunkerClient.ChunkCrawledWebpages(webpages);

            // Embeddings
            documents = await OpenAIEmbeddingsClient.RetrieveEmbeddingsAsync(documents);

            var documentsToDelete = GetOutdatedDocumentsFromBatch(batch);

            await DeleteDocumentsAsync(documentsToDelete);
            await UploadDocumentsAsync(documents);

            _logger.LogInformation("Batch {index} complete!", index);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed batch {index}!", index);
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
            var documents = Index.Where(d => batch.Any(item => item.URL == d.URL)).ToList();

            _logger.LogInformation($"Retrieved {{count}} outdated document{Grammar.GetPlurality(documents.Count, "", "s")}!", documents.Count);
            return documents;
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
            var documents = Index.Where(d => SitemapItems.All(item => d.URL != item.URL)).ToList();

            _logger.LogInformation($"Retrieved {{count}} invalid document{Grammar.GetPlurality(documents.Count, "", "s")}!", documents.Count);
            return documents;
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
        _logger.LogInformation($"Deleting {{count}} document{Grammar.GetPlurality(documents.Count, "", "s")}...", documents.Count);

        try
        {
            if (documents.Count > 0)
                await IndexClient.IndexDocumentsAsync(documents, IndexDocumentsAction.Delete);

            _logger.LogInformation($"Deleted {{count}} document{Grammar.GetPlurality(documents.Count, "", "s")}!", documents.Count);
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
        _logger.LogInformation($"Uploading {{count}} document{Grammar.GetPlurality(documents.Count, "", "s")}...", documents.Count);

        try
        {
            if (documents.Count > 0)
                await IndexClient.IndexDocumentsAsync(documents, IndexDocumentsAction.MergeOrUpload);

            _logger.LogInformation($"Uploaded {{count}} document{Grammar.GetPlurality(documents.Count, "", "s")}!", documents.Count);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed uploading documents!");
            throw;
        }
    }
}