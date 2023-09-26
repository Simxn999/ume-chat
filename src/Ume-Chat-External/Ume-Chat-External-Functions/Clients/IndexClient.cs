using System.Diagnostics;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;

namespace Ume_Chat_External_Functions.Clients;

/// <summary>
///     Client for handling the database/index.
/// </summary>
[DebuggerDisplay("{Index}")]
public class IndexClient
{
    private readonly ILogger _logger;

    public IndexClient(ILogger logger)
    {
        try
        {
            _logger = logger;

            Key = Variables.Get("COGNITIVE_SEARCH_API_KEY");
            URL = Variables.Get("COGNITIVE_SEARCH_URL");
            Index = Variables.Get("COGNITIVE_SEARCH_INDEX_NAME");

            var searchIndexClient = new SearchIndexClient(new Uri(URL), new AzureKeyCredential(Key));
            SearchClient = searchIndexClient.GetSearchClient(Index);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed creating IndexClient!");
            throw;
        }
    }

    /// <summary>
    ///     URL to database/index.
    /// </summary>
    private string URL { get; }

    /// <summary>
    ///     Key to database/index.
    /// </summary>
    private string Key { get; }

    /// <summary>
    ///     Name of database/index.
    /// </summary>
    private string Index { get; }

    /// <summary>
    ///     Client for handling the database/index.
    /// </summary>
    private SearchClient SearchClient { get; }

    /// <summary>
    ///     Send action to database with documents.
    /// </summary>
    /// <param name="documents">Documents to action</param>
    /// <param name="action">Action for database</param>
    public async Task IndexDocumentsAsync(IEnumerable<Document> documents,
                                          Func<Document, IndexDocumentsAction<Document>> action)
    {
        try
        {
            var actions = documents.Select(action).ToArray();
            var batch = IndexDocumentsBatch.Create(actions);
            await SearchClient.IndexDocumentsAsync(batch, new IndexDocumentsOptions { ThrowOnAnyError = true });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed indexing documents!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve documents from database with optional filter.
    /// </summary>
    /// <param name="filter">Optional: Documents filter</param>
    /// <returns>Enumerable of documents from database</returns>
    public async Task<IEnumerable<Document>> GetDocumentsAsync(string? filter = null)
    {
        var documentsCount = await GetDocumentsCountAsync();
        _logger.LogInformation("Retrieving {count} documents...", documentsCount);

        try
        {
            var options = new SearchOptions { Filter = filter ?? "" };

            Response<SearchResults<Document>>? result = await SearchClient.SearchAsync<Document>(string.Empty, options);
            var documents = result.Value.GetResults().Select(item => item.Document);

            _logger.LogInformation("Retrieved {count} documents!", documentsCount);
            return documents;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of documents!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve documents from database with only necessary fields.
    /// </summary>
    /// <returns>List of documents from database</returns>
    public async Task<List<Document>> GetDocumentsForComparisonAsync()
    {
        var documentsCount = await GetDocumentsCountAsync();
        _logger.LogInformation("Retrieving {count} documents...", documentsCount);

        try
        {
            var options = new SearchOptions
                          {
                              // Necessary fields
                              Select =
                              {
                                  "id",
                                  "url",
                                  "title",
                                  "lastmod",
                                  "chunk_id"
                              },
                              Size = documentsCount
                          };
            var result = await SearchClient.SearchAsync<Document>(string.Empty, options);
            var documents = result.Value.GetResults().Select(item => item.Document).ToList();

            return documents;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of documents!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the total amount of documents inside of database/index.
    /// </summary>
    /// <returns>Integer of amount of documents in index</returns>
    public async Task<int> GetDocumentsCountAsync()
    {
        try
        {
            return (int)await SearchClient.GetDocumentCountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of document count!");
            throw;
        }
    }
}
