using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
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

            ChunkSize = Variables.GetInt("CHUNK_SIZE");
            ChunkOverlap = (int)(ChunkSize * Variables.GetFloat("CHUNK_OVERLAP_MULTIPLIER"));
            ExcludedContent = Variables.GetEnumerable("CHUNKER_EXCLUDED_CONTENT");
            ChunkSplitters = new Dictionary<string, string>
                             {
                                 { "#", "\n\n" },
                                 { "\n\n", "\n" },
                                 { "\n", "." },
                                 { ".", "?" },
                                 { "?", "!" },
                                 { "!", " " },
                                 { " ", string.Empty }
                             };
            SentenceSplitters = new[] { '\n', '.', '!', '?' };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed creating ChunkerClient!");
            throw;
        }
    }

    /// <summary>
    ///     Size of chunks in amount of characters.
    /// </summary>
    private int ChunkSize { get; }

    /// <summary>
    ///     Size of overlap between chunks in amount of characters.
    /// </summary>
    private int ChunkOverlap { get; }

    /// <summary>
    ///     Enumerable of strings that should be removed from data.
    /// </summary>
    private IEnumerable<string> ExcludedContent { get; }

    /// <summary>
    ///     Dictionary of strings to recursively split chunks by.
    /// </summary>
    private Dictionary<string, string> ChunkSplitters { get; }

    /// <summary>
    ///     Array of characters that determine the ending of a sentence.
    /// </summary>
    private char[] SentenceSplitters { get; }

    /// <summary>
    ///     Splits content of crawled webpages into chunks in the form of documents.
    /// </summary>
    /// <param name="crawledWebpages">List of crawled webpages</param>
    /// <returns>List of documents each containing a chunk</returns>
    public List<Document> ChunkCrawledWebpages(IList<CrawledWebpage> crawledWebpages)
    {
        _logger.LogInformation($"Chunking {{Count}} webpage{Grammar.GetPlurality(crawledWebpages.Count, "", "s")}...",
                               crawledWebpages.Count);

        try
        {
            // Chunk every webpage synchrnonously
            var tasks = crawledWebpages.Select(w => Task.Run(() => ChunkCrawledWebpage(w))).ToList();

            // Wait for chunking to complete
            Task.WaitAll(tasks.Cast<Task>().ToArray());

            return tasks.SelectMany(t => t.Result).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed chunking webpages!");
            throw;
        }
    }

    /// <summary>
    ///     Splits the content of a webpage into chunks in the form of documents.
    /// </summary>
    /// <param name="crawledWebpage">CrawledWebpage to chunk</param>
    /// <returns>Enumerable of documents each containing a chunk</returns>
    private IEnumerable<Document> ChunkCrawledWebpage(CrawledWebpage crawledWebpage)
    {
        try
        {
            var output = new List<Document>();

            // Retrieve chunks of content
            var chunks = ChunkContentRecursive(crawledWebpage.Content, ChunkSplitters["#"]).ToList();

            ApplyOverlappingToChunks(ref chunks);
            TrimChunks(ref chunks);

            // Convert chunks to documents
            output.AddRange(chunks.Select((chunk, i) => new Document
                                                        {
                                                            URL = crawledWebpage.URL,
                                                            Title = crawledWebpage.Title,
                                                            Path = crawledWebpage.Path,
                                                            Content = chunk,
                                                            ChunkID = i,
                                                            LastModified = crawledWebpage.LastModified,
                                                            Priority = crawledWebpage.Priority
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
            FilterContent(ref content);

            var chunks = new List<string>();
            var stack = SplitContentIntoStack(content, splitter);

            // StringBuilder containing the chunk
            var sb = new StringBuilder();
            while (stack.Count > 0)
            {
                var currentCharacterCount = stack.Peek().Length;
                var nextCharacterCount = sb.Length + stack.Peek().Length;

                // If current chunk is too big
                if (currentCharacterCount > ChunkSize)
                {
                    // If out of splitters, throw
                    if (string.IsNullOrEmpty(ChunkSplitters[splitter]))
                        throw new Exception("Reached last chunking splitter!");

                    // Split the chunk with the next splitter
                    foreach (var chunk in ChunkContentRecursive(stack.Pop(), ChunkSplitters[splitter]))
                        stack.Push(chunk);

                    continue;
                }

                // If chunk size limit is reached
                if (currentCharacterCount <= ChunkSize && nextCharacterCount > ChunkSize)
                {
                    // Register the chunk and move on to the next
                    chunks.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                // Add current string to string builder
                sb.Append(stack.Pop());
            }

            chunks.Add(sb.ToString());

            if (splitter == ChunkSplitters["#"])
                CombineSmallChunksWithAdjacent(ref chunks);

            return chunks;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed recursive chunking of content!");
            throw;
        }
    }

    /// <summary>
    ///     Remove unwanted content from text.
    /// </summary>
    /// <param name="content">Text to remove content from</param>
    private void FilterContent(ref string content)
    {
        foreach (var value in ExcludedContent)
            content = content.Replace(value, string.Empty);

        content = content.Replace(" \n", "\n");
    }

    /// <summary>
    ///     Split content by provided splitter and convert it to a stack.
    /// </summary>
    /// <param name="content">Content to split</param>
    /// <param name="splitter">String to split content by</param>
    /// <returns>Stack containing all split strings including the splitters</returns>
    private static Stack<string> SplitContentIntoStack(string content, string splitter)
    {
        // Regex pattern, examples: (\.), (\!), (\n)
        // Remove leading "\" for linebreaks
        var pattern = @$"(\{(splitter[0] == '\\' ? splitter[1..] : splitter)})";
        var chunks = Regex.Split(content, pattern).Reverse().Where(s => !string.IsNullOrEmpty(s));

        return new Stack<string>(chunks);
    }

    /// <summary>
    ///     Searches for small chunks and combines them with the chunk next to it.
    ///     This eliminates documents in the database that
    ///     most likely does not contain any contextual information.
    /// </summary>
    /// <param name="chunks">List of strings that are chunks</param>
    private void CombineSmallChunksWithAdjacent(ref List<string> chunks)
    {
        if (chunks.Count == 1)
            return;

        // Backwards search through chunks if there are more than one
        for (var i = chunks.Count - 1; i >= 0; i--)
        {
            var chunkSize = chunks[i].Length;

            // If chunk is not small, move on
            if (chunkSize > ChunkOverlap)
                continue;

            // If the chunk is first, insert current to next
            if (i == 0)
            {
                chunks[i + 1] = chunks[i] + chunks[i + 1];
                chunks.RemoveAt(i);

                continue;
            }

            // Add current chunk to previous
            chunks[i - 1] += chunks[i];
            chunks.RemoveAt(i);
        }
    }

    /// <summary>
    ///     Apply overlapping to chunks with the purpose to contain context between chunks.
    /// </summary>
    /// <param name="chunks">Reference to list of chunks to apply overlapping on</param>
    private void ApplyOverlappingToChunks(ref List<string> chunks)
    {
        // Apply overlap for all chunks but last
        for (var i = 0; i < chunks.Count - 1; i++)
            chunks[i] += GetOverlapFromChunk(chunks[i + 1]);
    }

    /// <summary>
    ///     Trim the sides of every string inside of provided list of chunks.
    /// </summary>
    /// <param name="chunks">Reference to the list of chunks</param>
    private static void TrimChunks(ref List<string> chunks)
    {
        chunks = chunks.Select(c => c.Trim()).ToList();
    }

    /// <summary>
    ///     Retrieve overlap substring from a chunk.
    /// </summary>
    /// <param name="chunk">Chunk to retrieve overlap from</param>
    /// <returns>String of content that should overlap</returns>
    private string GetOverlapFromChunk(string chunk)
    {
        var indexes = GetIndexesOfSentenceSplitters(chunk);

        // Retrieve the index of the splitter closest to the chunk overlap limit
        var sentenceSplitterIndex = indexes.OrderBy(n => Math.Abs(ChunkOverlap - n)).FirstOrDefault() + 1;

        // If no sentence splitter was found
        if (sentenceSplitterIndex == 1)
            // Return the first [ChunkOverlap] characters
            return chunk[..ChunkOverlap] + "...";

        // Return overlap
        return chunk[..sentenceSplitterIndex];
    }

    /// <summary>
    ///     <para>Search for and retrieve positions of sentence splitters close to ChunkOverlap inside of a chunk of text.</para>
    ///     <para>Sentence splitters examples: '.', '!', '?'</para>
    /// </summary>
    /// <param name="chunk">Chunk of text</param>
    /// <returns>HashSet with positions of sentence splitters in the chunk.</returns>
    private HashSet<int> GetIndexesOfSentenceSplitters(string chunk)
    {
        var indexes = new HashSet<int>();
        var startIndex = ChunkOverlap / 2;
        var endIndex = startIndex + ChunkOverlap;

        // Look for sentence splitters that are close to the chunk overlap limit
        for (var i = startIndex; i <= endIndex && i < chunk.Length; i++)
            if (SentenceSplitters.Contains(chunk[i]))
                indexes.Add(i);

        return indexes;
    }
}
