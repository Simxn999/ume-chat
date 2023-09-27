using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;

namespace Ume_Chat_External_Functions.Clients;

/// <summary>
///     Client for handling embeddings.
/// </summary>
[DebuggerDisplay("{OpenAIURL}")]
public class OpenAIEmbeddingsClient
{
    private readonly ILogger _logger;

    public OpenAIEmbeddingsClient(ILogger logger)
    {
        try
        {
            _logger = logger;

            OpenAIAPIKey = Variables.Get("OPENAI_API_KEY");

            OpenAIURL = Variables.Get("OPENAI_URL");
            EmbeddingsDeployment = Variables.Get("OPENAI_EMBEDDINGS_DEPLOYMENT");
            EmbeddingsBatchSize = Variables.GetInt("OPENAI_EMBEDDINGS_BATCH_SIZE");

            Client = new OpenAIClient(new Uri(OpenAIURL), new AzureKeyCredential(OpenAIAPIKey));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed creation of OpenAIEmbeddingsClient!");
            throw;
        }
    }

    /// <summary>
    ///     Key to Azure OpenAI Service.
    /// </summary>
    private string OpenAIAPIKey { get; }

    /// <summary>
    ///     URL to Azure OpenAI Service.
    /// </summary>
    private string OpenAIURL { get; }

    /// <summary>
    ///     Azure OpenAI embeddings deployment.
    /// </summary>
    private string EmbeddingsDeployment { get; }

    /// <summary>
    ///     Embeddings request batch limit.
    ///     2023-09-22: 16
    /// </summary>
    private int EmbeddingsBatchSize { get; }

    /// <summary>
    ///     Client for handling requests to OpenAI Service.
    /// </summary>
    private OpenAIClient Client { get; }

    /// <summary>
    ///     Populate documents with embeddings based on it's content.
    /// </summary>
    /// <param name="documents">Documents to populate embeddings</param>
    /// <returns>List of documents populated with embeddings</returns>
    public async Task<List<Document>> RetrieveEmbeddingsAsync(ICollection<Document> documents)
    {
        _logger.LogInformation($"Retrieving embedding{Grammar.GetPlurality(documents.Count, "", "s")} for document{Grammar.GetPlurality(documents.Count, "", "s")}...");

        try
        {
            var output = new List<Document>();

            // Group documents by URL
            var urlGroups = documents.GroupBy(d => d.URL, d => d).ToList();

            for (var i = 0; i < urlGroups.Count; i++)
            {
                // Retrieve all documents with current URL
                var groupDocuments = documents.Where(d => d.URL == urlGroups[i].Key).ToList();
                groupDocuments =
                    (await PopulateDocumentsByURLWithEmbeddingsAsync(groupDocuments, i + 1, urlGroups.Count)).ToList();

                output.AddRange(groupDocuments);
            }

            _logger.LogInformation($"Retrieved embedding{Grammar.GetPlurality(documents.Count, "", "s")} for {{count}} document{Grammar.GetPlurality(documents.Count, "", "s")}!",
                                   output.Count);
            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of embeddings!");
            throw;
        }
    }

    /// <summary>
    ///     Populate a webpage's documents with embedding based on it's content.
    /// </summary>
    /// <param name="documents">Webpage's documents to populate with embeddings</param>
    /// <param name="index">Current index of webpage</param>
    /// <param name="total">Total number of webpages</param>
    /// <returns>Enumerable of documents populated with embeddings</returns>
    private async Task<IEnumerable<Document>> PopulateDocumentsByURLWithEmbeddingsAsync(
        IList<Document> documents,
        int index,
        int total)
    {
        var url = documents.FirstOrDefault()?.URL ?? "[URL NOT FOUND]";
        _logger.LogInformation($"{new ProgressString(index, total)} Retrieving {{count}} embedding{Grammar.GetPlurality(documents.Count, "", "s")} for \"{{url}}\"...",
                               documents.Count,
                               url);

        try
        {
            var embeddings = new List<EmbeddingItem>();

            // Batch documents based on [EmbeddingsBatchSize]
            var batches = documents.Chunk(EmbeddingsBatchSize).Select(c => c.Select(d => d.Content));

            foreach (var batch in batches)
            {
                var response = await Client.GetEmbeddingsAsync(EmbeddingsDeployment, new EmbeddingsOptions(batch));

                embeddings.AddRange(response.Value.Data);
            }

            for (var i = 0; i < embeddings.Count; i++)
                documents[i].Vector = embeddings[i].Embedding;

            return documents;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of embeddings for \"{url}\"!", url);
            throw;
        }
    }
}
