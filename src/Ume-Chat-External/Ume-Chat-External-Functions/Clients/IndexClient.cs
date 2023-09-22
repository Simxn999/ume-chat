using System.Diagnostics;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;

namespace Ume_Chat_External_Functions.Clients;

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

    private string URL { get; }
    private string Key { get; }
    private string Index { get; }
    private SearchClient SearchClient { get; }

    public async Task IndexDocumentsAsync(IEnumerable<Document> documents, Func<Document, IndexDocumentsAction<Document>> action)
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

    public async Task<IEnumerable<Document>> GetDocumentsAsync(string? filter = null)
    {
        var documentsCount = await GetDocumentsCountAsync();
        _logger.LogInformation("Retrieving {count} documents...", documentsCount);

        try
        {
            var options = new SearchOptions
                          {
                              Filter = filter ?? ""
                          };

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

    public async Task<List<Document>> GetDocumentsForComparisonAsync()
    {
        var documentsCount = await GetDocumentsCountAsync();
        _logger.LogInformation("Retrieving {count} documents...", documentsCount);

        try
        {
            var options = new SearchOptions
                          {
                              Select = { "id", "url", "title", "lastmod", "chunk_id" },
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