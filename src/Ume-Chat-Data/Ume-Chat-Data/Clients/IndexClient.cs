using System.Diagnostics;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Ume_Chat_Models.Ume_Chat_Data.Ume_Chat_Data;
using Ume_Chat_Utilities;

namespace Ume_Chat_Data.Clients;

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
            DefaultFields = Variables.GetEnumerable("COGNITIVE_SEARCH_INDEX_DEFAULT_FIELDS");
            DefaultGroups = Variables.GetEnumerable("DOCUMENT_DEFAULT_GROUPS");

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
    ///     Default fields used with document retrieval.
    /// </summary>
    private IEnumerable<string> DefaultFields { get; }

    /// <summary>
    ///     Default groups used on documents.
    /// </summary>
    private IEnumerable<string> DefaultGroups { get; }

    /// <summary>
    ///     Client for handling the database/index.
    /// </summary>
    private SearchClient SearchClient { get; }

    /// <summary>
    ///     Send action to database with documents.
    /// </summary>
    /// <param name="documents">Documents to action</param>
    /// <param name="action">Action for database</param>
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

    /// <summary>
    ///     Retrieve documents from database with optional filter and field selection.
    /// </summary>
    /// <param name="filter">Optional: Documents filter</param>
    /// <param name="select">Optional: Fields to retrieve along with documents</param>
    /// <param name="size">Optional: Amount of documents to retrieve</param>
    /// <param name="groups">Optional: Group of documents to retrieve</param>
    /// <returns>Enumerable of documents from database</returns>
    public async Task<IEnumerable<Document>> GetDocumentsAsync(string? filter = null,
                                                               IEnumerable<string>? select = null,
                                                               int? size = null,
                                                               IEnumerable<string>? groups = null)
    {
        _logger.LogInformation("Retrieving documents...");

        try
        {
            var options = GetSearchOptions(filter, select, size, groups);

            return await SearchAsync(options);
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
            var select = new[] { "lastmod" };
            var options = GetSearchOptions(select: select, size: documentsCount);

            return await SearchAsync(options);
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

    /// <summary>
    ///     Search the index with provided options.
    /// </summary>
    /// <param name="options">Search options</param>
    /// <returns>List of documents found in index</returns>
    private async Task<List<Document>> SearchAsync(SearchOptions options)
    {
        try
        {
            var result = await SearchClient.SearchAsync<Document>(string.Empty, options);
            return result.Value.GetResults().Select(item => item.Document).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed index search!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve search options for index search.
    /// </summary>
    /// <param name="filter">Optional: Documents filter</param>
    /// <param name="select">Optional: Fields to retrieve along with documents</param>
    /// <param name="size">Optional: Amount of documents to retrieve</param>
    /// <param name="groups">Optional: Group of documents to retrieve</param>
    /// <returns>Configured SearchOptions</returns>
    private SearchOptions GetSearchOptions(string? filter = null, IEnumerable<string>? select = null, int? size = null, IEnumerable<string>? groups = null)
    {
        try
        {
            var options = new SearchOptions();

            // Select
            foreach (var defaultField in DefaultFields)
                options.Select.Add(defaultField);

            foreach (var field in select ?? Enumerable.Empty<string>())
            {
                if (options.Select.Contains(field))
                    continue;

                options.Select.Add(field);
            }

            // Size
            options.Size = size;

            // Filter & Groups
            options.Filter = $"{GetGroupsFilter(groups)}{(filter is not null ? $"and {filter}" : string.Empty)}";

            return options;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed configuration of SearchOptions!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the proper filter format for document groups.
    /// </summary>
    /// <param name="groups">Enumerable of groups</param>
    /// <returns>String as filter query for document groups</returns>
    private string GetGroupsFilter(IEnumerable<string>? groups = null)
    {
        try
        {
            return $"group_ids/any(g:search.in(g, '{string.Join(", ", groups ?? DefaultGroups)}'))";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of groups filter!");
            throw;
        }
    }
}
