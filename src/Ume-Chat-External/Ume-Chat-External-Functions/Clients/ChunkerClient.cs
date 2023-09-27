using System.Text;
using Microsoft.Extensions.Logging;
using SharpToken;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;

namespace Ume_Chat_External_Functions.Clients;

/// <summary>
///     Client for splitting text content into smaller chunks.
/// </summary>
public class ChunkerClient
{
    private readonly ILogger _logger;

    public ChunkerClient(ILogger logger)
    {
        try
        {
            _logger = logger;

            ChunkOverlap = Variables.GetInt("CHUNK_OVERLAP_SIZE");
            ChunkSize = Variables.GetInt("CHUNK_SIZE") - ChunkOverlap;
            TokenizerEncodingModel = Variables.Get("TOKENIZER_ENCODING_MODEL");
            ExcludedContent = Variables.GetEnumerable("CHUNKER_EXCLUDED_CONTENT");
            Tokenizer = GptEncoding.GetEncoding(TokenizerEncodingModel);
            ChunkSplitters = new Dictionary<string, string>
                             {
                                 { "#", "\n\n" },
                                 { "\n\n", "\n" },
                                 { "\n", " " },
                                 { " ", " " }
                             };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed creating ChunkerClient!");
            throw;
        }
    }

    /// <summary>
    ///     Size of chunks in amount of tokens.
    /// </summary>
    private int ChunkSize { get; }

    /// <summary>
    ///     Size of overlap between chunks in amount of tokens.
    /// </summary>
    private int ChunkOverlap { get; }

    /// <summary>
    ///     Encoding model for converting text into tokens used by OpenAI.
    /// </summary>
    private string TokenizerEncodingModel { get; }

    /// <summary>
    ///     Enumerable of strings that should be removed from data.
    /// </summary>
    private IEnumerable<string> ExcludedContent { get; }

    /// <summary>
    ///     Dictionary of strings to recursively split chunks by.
    /// </summary>
    private Dictionary<string, string> ChunkSplitters { get; }

    /// <summary>
    ///     Client to manage tokens used by OpenAI.
    /// </summary>
    private GptEncoding Tokenizer { get; }

    /// <summary>
    ///     Splits content of crawled webpages into chunks in the form of documents.
    /// </summary>
    /// <param name="crawledWebpages">List of crawled webpages</param>
    /// <returns>List of documents each containing a chunk</returns>
    public List<Document> ChunkCrawledWebpages(IList<CrawledWebpage> crawledWebpages)
    {
        _logger.LogInformation("Chunking crawled webpages...");

        try
        {
            var output = new List<Document>();

            // Crawl every webpage and add it to the output list
            for (var i = 0; i < crawledWebpages.Count; i++)
                output.AddRange(ChunkCrawledWebpage(crawledWebpages[i], i + 1, crawledWebpages.Count));

            _logger.LogInformation("Chunked crawled webpages!");
            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed chunking crawled webpages!");
            throw;
        }
    }

    /// <summary>
    ///     Splits the content of a webpage into chunks in the form of documents.
    /// </summary>
    /// <param name="crawledWebpage">CrawledWebpage to chunk</param>
    /// <param name="index">Current index of webpage in batch</param>
    /// <param name="total">Total number of webpages in batch</param>
    /// <returns>Enumerable of documents each containing a chunk</returns>
    private IEnumerable<Document> ChunkCrawledWebpage(CrawledWebpage crawledWebpage, int index, int total)
    {
        _logger.LogInformation($"{new ProgressString(index, total)} Chunking \"{{url}}\"...", crawledWebpage.URL);

        try
        {
            var output = new List<Document>();

            // Retrieve chunks of content
            var chunks = ChunkContentRecursive(crawledWebpage.Content, ChunkSplitters["#"]).ToList();

            // Convert chunks into list of tokens
            var encodedChunks = chunks.Select(c => Tokenizer.Encode(c)).ToList();

            // Apply overlapping to chunks
            for (var i = 0; i < chunks.Count - 1; i++)
            {
                // Retrieve the first [ChunkOverlap] tokens of next chunk
                var overlap = encodedChunks[i + 1].Take(ChunkOverlap).ToList();

                // Append those tokens to the current chunk
                chunks[i] += Tokenizer.Decode(overlap) + "...";
            }

            // Convert chunks to documents
            output.AddRange(chunks.Select((chunk, i) => new Document
                                                        {
                                                            URL = crawledWebpage.URL,
                                                            Title = crawledWebpage.Title,
                                                            Content = chunk,
                                                            ChunkID = i,
                                                            LastModified = crawledWebpage.LastModified
                                                        }));

            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed chunking \"{url}\"!", crawledWebpage.URL);
            throw;
        }
    }

    /// <summary>
    ///     Chunk text content recursively by specified splitting string.
    ///     TODO: Optimize!
    /// </summary>
    /// <param name="content">Text content to chunk</param>
    /// <param name="splitter">String to split by</param>
    /// <returns>Enumerable of chunks</returns>
    private IEnumerable<string> ChunkContentRecursive(string content, string splitter)
    {
        try
        {
            var chunks = new List<string>();

            // Remove irrelevant content
            content = ExcludedContent.Aggregate(content,
                                                (current, excludedContent) => current.Replace(excludedContent, ""));

            // Remove leading whitespace from linebreaks
            content = content.Replace(" \n", "\n");

            var stack = new Stack<string>(content.Split(splitter).Reverse());

            // StringBuilder containing the chunk
            var sb = new StringBuilder();
            while (stack.Count > 0)
            {
                var currentTokenCount = GetTokenCount(stack.Peek() + splitter);
                var nextTokenCount = GetTokenCount(sb + stack.Peek() + splitter);

                // If current chunk is too big
                if (currentTokenCount > ChunkSize)
                {
                    // Split the chunk with the next splitter
                    foreach (var chunk in ChunkContentRecursive(stack.Pop(), ChunkSplitters[splitter]))
                        stack.Push(chunk);

                    continue;
                }

                // If chunk size limit is reached
                if (currentTokenCount <= ChunkSize && nextTokenCount > ChunkSize)
                {
                    // Register the chunk and move on to the next
                    chunks.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                sb.Append(stack.Pop() + splitter);
            }

            chunks.Add(sb.ToString().Trim('\n'));

            return chunks;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed recursive chunking of content!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve the amount of tokens of input.
    /// </summary>
    /// <param name="input">String to count tokens on</param>
    /// <returns>Integer of amount of tokens of input</returns>
    private int GetTokenCount(string input)
    {
        try
        {
            return Tokenizer.Encode(input).Count;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of token count!");
            throw;
        }
    }
}
