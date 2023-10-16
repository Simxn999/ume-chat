using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Logging;
using Ume_Chat_Models.Ume_Chat_Data.Ume_Chat;
using Ume_Chat_Utilities;

namespace Ume_Chat_Data.Clients;

/// <summary>
///     Client for handling extraction of keywords through Azure Language Service.
/// </summary>
public class KeywordsClient
{
    private readonly ILogger _logger;

    public KeywordsClient(ILogger logger)
    {
        try
        {
            _logger = logger;

            Key = Variables.Get("TEXT_ANALYTICS_API_KEY");
            URL = Variables.Get("TEXT_ANALYTICS_URL");
            DefaultLanguage = Variables.Get("TEXT_ANALYTICS_DEFAULT_LANGUAGE");
            LanguageURLSegments = Variables.GetEnumerable("TEXT_ANALYTICS_LANGUAGE_URL_SEGMENTS");
            ExcludedLanguages = Variables.GetEnumerable("TEXT_ANALYTICS_EXCLUDED_LANGUAGES");
            KeywordsBatchSize = Variables.GetInt("TEXT_ANALYTICS_REQUEST_KEYWORDS_BATCH_SIZE");
            DetectLanguagesBatchSize = Variables.GetInt("TEXT_ANALYITCS_REQUEST_LANGUAGE_DETECTION_BATCH_SIZE");
            Client = new TextAnalyticsClient(new Uri(URL), new AzureKeyCredential(Key));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed creating KeywordsClient!");
            throw;
        }
    }

    /// <summary>
    ///     Key to Azure Language Service.
    /// </summary>
    private string Key { get; }

    /// <summary>
    ///     URL to Azure Language Service.
    /// </summary>
    private string URL { get; }

    /// <summary>
    ///     Default language code - Swedish.
    /// </summary>
    private string DefaultLanguage { get; }

    /// <summary>
    ///     Segments of URLs where the language is not default/Swedish.
    /// </summary>
    private IEnumerable<string> LanguageURLSegments { get; }

    /// <summary>
    ///     Languages that are not supported by keywords extractor.
    /// </summary>
    private IEnumerable<string> ExcludedLanguages { get; }

    /// <summary>
    ///     Maximum amount of inputs for keywords extractor.
    /// </summary>
    private int KeywordsBatchSize { get; }

    /// <summary>
    ///     Maximum amount of inputs for language detection.
    /// </summary>
    private int DetectLanguagesBatchSize { get; }

    /// <summary>
    ///     Client for handling requests to Azure Language Service.
    /// </summary>
    private TextAnalyticsClient Client { get; }

    /// <summary>
    ///     Populate documents with extracted keywords based on Title & Content.
    /// </summary>
    /// <param name="documents">Reference to list of documents to populate & extract keywords</param>
    public void PopulateDocumentsWithKeywords(ref List<Document> documents)
    {
        try
        {
            _logger.LogInformation($"Populating {{Count}} document{Grammar.GetPlurality(documents.Count, "", "s")} with keywords...",
                                   documents.Count);

            var foreignLanguages = GetForeignLanguages(documents);

            var titleKeywordsTask = ExtractKeywordsAsync(documents, foreignLanguages, KeywordsType.Title);
            var contentKeywordsTask = ExtractKeywordsAsync(documents, foreignLanguages, KeywordsType.Content);

            Task.WaitAll(titleKeywordsTask, contentKeywordsTask);

            foreach (var document in documents)
            {
                document.KeywordsTitle = titleKeywordsTask.Result[document.URL ?? string.Empty];
                document.KeywordsContent = contentKeywordsTask.Result[document.ID];
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed populating documents with keywords!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the language of foreign documents.
    /// </summary>
    /// <param name="documents">Documents to look for foreign languages</param>
    /// <returns>Dictionary where key is Document.ID and value is language code</returns>
    public Dictionary<string, string> GetForeignLanguages(List<Document> documents)
    {
        try
        {
            var detectLanguageInputs = ConvertDocumentsToDetectLanguageInputs(documents);
            var batches = BatchDetectLanguageInputs(detectLanguageInputs);

            var tasks = batches.Select(b => Client.DetectLanguageBatchAsync(b)).ToList();

            Task.WaitAll(tasks.Cast<Task>().ToArray());

            var output = tasks.SelectMany(t => t.Result.Value)
                              .Where(x => !ExcludedLanguages.Contains(x.PrimaryLanguage.Iso6391Name))
                              .ToDictionary(x => x.Id, x => x.PrimaryLanguage.Iso6391Name);

            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of foreign languages!");
            throw;
        }
    }

    /// <summary>
    ///     Extracts keywords from documents based on the provided type (Title/Content).
    /// </summary>
    /// <param name="documents">Documents to extract keywords from</param>
    /// <param name="foreignLanguages">Dictionary of IDs with foreign languages</param>
    /// <param name="type">Type of keywords extraction. Title or Content</param>
    /// <returns>
    ///     <para>Title = Dictionary where key is Document.URL and value is keywords</para>
    ///     <para>Content = Dictionary where key is Document.ID and value is keywords</para>
    /// </returns>
    private async Task<Dictionary<string, KeyPhraseCollection>> ExtractKeywordsAsync(
        List<Document> documents,
        Dictionary<string, string> foreignLanguages,
        KeywordsType type)
    {
        try
        {
            var output = new Dictionary<string, KeyPhraseCollection>();
            var textDocumentInputs = ConvertDocumentsToTextDocumentInputs(documents, foreignLanguages, type);
            var batches = BatchTextDocumentInputs(textDocumentInputs);

            foreach (var batch in batches)
            {
                var keywordsBatch = await Client.ExtractKeyPhrasesBatchAsync(batch);

                foreach (var result in keywordsBatch.Value)
                    output.TryAdd(result.Id, result.KeyPhrases);
            }

            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed keywords extraction!");
            throw;
        }
    }

    /// <summary>
    ///     Batches list of DetectLanguageInput into required max batch size.
    /// </summary>
    /// <param name="detectLanguageInputs">List of DetectLanguageInput to batch</param>
    /// <returns>List containing batches of DetectLanguageInput</returns>
    private List<List<DetectLanguageInput>> BatchDetectLanguageInputs(List<DetectLanguageInput> detectLanguageInputs)
    {
        try
        {
            return detectLanguageInputs.Chunk(DetectLanguagesBatchSize).Select(b => b.ToList()).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed batching language detection inputs!");
            throw;
        }
    }

    /// <summary>
    ///     Batches list of TextDocumentInput into required max batch size.
    /// </summary>
    /// <param name="textDocumentInputs">List of TextDocumentInput to batch</param>
    /// <returns>List containing batches of TextDocumentInput</returns>
    private List<List<TextDocumentInput>> BatchTextDocumentInputs(List<TextDocumentInput> textDocumentInputs)
    {
        try
        {
            return textDocumentInputs.Chunk(KeywordsBatchSize).Select(c => c.ToList()).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed batching text document inputs!");
            throw;
        }
    }

    /// <summary>
    ///     Converts a list of documents to a list of DetectLanguageInput.
    /// </summary>
    /// <param name="documents">Documents to convert</param>
    /// <returns>List of DetectLanguageInput</returns>
    private List<DetectLanguageInput> ConvertDocumentsToDetectLanguageInputs(List<Document> documents)
    {
        try
        {
            return documents.Where(d => d.URL is not null && LanguageURLSegments.Any(s => d.URL.Contains(s)))
                            .Select(d => new DetectLanguageInput(d.ID, d.Content))
                            .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed converting documents to language detection inputs!");
            throw;
        }
    }

    /// <summary>
    ///     Converts a list of documents to a list of TextDocumentInput.
    /// </summary>
    /// <param name="documents">Documents to convert</param>
    /// <param name="foreignLanguages">Dictionary of IDs with foreign language</param>
    /// <param name="type">Type of keywords extraction. Title or Content</param>
    /// <returns>List of TextDocumentInput</returns>
    private List<TextDocumentInput> ConvertDocumentsToTextDocumentInputs(
        List<Document> documents,
        Dictionary<string, string> foreignLanguages,
        KeywordsType type)
    {
        try
        {
            return documents.Select(d =>
                                    {
                                        var input = ConvertDocumentToTextDocumentInput(d, type);

                                        if (foreignLanguages.ContainsKey(d.ID))
                                            input.Language = foreignLanguages[d.ID];

                                        return input;
                                    })
                             // Distinct because Titles are batched by URL since documents with the same URL has the same Title
                            .DistinctBy(x => x.Id)
                            .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed converting documents to text document inputs!");
            throw;
        }
    }

    /// <summary>
    ///     Converts a document to TextDocumentInput.
    /// </summary>
    /// <param name="document">Document to convert</param>
    /// <param name="type">Type of keywords extraction. Title or Content</param>
    /// <returns>
    ///     <para>Title = TextDocumentInput where Id = Document.URL</para>
    ///     <para>Content = TextDocumentInput where Id = Document.ID</para>
    /// </returns>
    /// <exception cref="Exception">KeywordsType was incorrect!</exception>
    private TextDocumentInput ConvertDocumentToTextDocumentInput(Document document, KeywordsType type)
    {
        try
        {
            var textDocumentInput = type switch
                                    {
                                        // URL because documents with the same URL has the same Title
                                        KeywordsType.Title => new TextDocumentInput(document.URL, document.Title),
                                        KeywordsType.Content => new TextDocumentInput(document.ID, document.Content),
                                        _ => throw new Exception("KeywordsType was incorrect!")
                                    };
            textDocumentInput.Language = DefaultLanguage;

            return textDocumentInput;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed converting document to text document input!");
            throw;
        }
    }
}
