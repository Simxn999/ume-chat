using System.Text;
using Microsoft.Extensions.Logging;
using SharpToken;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.Functions;

namespace Ume_Chat_External_Functions.Clients;

public class ChunkerClient
{
    private readonly ILogger _logger;

    public ChunkerClient(ILogger logger)
    {
        try
        {
            _logger = logger;

            ChunkOverlap = int.Parse(Variables.Get("CHUNK_OVERLAP_SIZE"));
            ChunkSize = int.Parse(Variables.Get("CHUNK_SIZE")) - ChunkOverlap;
            TokenizerEncodingModel = Variables.Get("TOKENIZER_ENCODING_MODEL");
            ExternalLinkTooltip = Variables.Get("EXTERNAL_LINK_TOOLTIP");
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

    private int ChunkSize { get; }
    private int ChunkOverlap { get; }
    private string TokenizerEncodingModel { get; }
    private string ExternalLinkTooltip { get; }
    private Dictionary<string, string> ChunkSplitters { get; }
    private GptEncoding Tokenizer { get; }

    public List<Document> ChunkCrawledWebpages(IList<CrawledWebpage> crawledWebpages)
    {
        _logger.LogInformation("Chunking crawled webpages...");

        try
        {
            var output = new List<Document>();

            for (var i = 0; i < crawledWebpages.Count; i++) output.AddRange(ChunkCrawledWebpage(crawledWebpages[i], i + 1, crawledWebpages.Count));

            _logger.LogInformation("Chunked crawled webpages!");
            return output;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed chunking crawled webpages!");
            throw;
        }
    }

    private IEnumerable<Document> ChunkCrawledWebpage(CrawledWebpage crawledWebpage, int index, int total)
    {
        _logger.LogInformation($"{new ProgressString(index, total)} Chunking \"{{url}}\"...", crawledWebpage.URL);

        try
        {
            var output = new List<Document>();

            var chunks = ChunkContentRecursive(crawledWebpage.Content, ChunkSplitters["#"]).ToList();
            var encodedChunks = chunks.Select(c => Tokenizer.Encode(c)).ToList();
            for (var i = 0; i < chunks.Count - 1; i++)
            {
                var overlap = encodedChunks[i + 1].Take(ChunkOverlap).ToList();
                chunks[i] += Tokenizer.Decode(overlap) + "...";
            }

            chunks[^1] = chunks[^1].Trim();

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

    private IEnumerable<string> ChunkContentRecursive(string content, string splitter)
    {
        try
        {
            var chunks = new List<string>();
            content = content.Replace(ExternalLinkTooltip, "");
            content = content.Replace(" \n", "\n");
            var stack = new Stack<string>(content.Split(splitter).Reverse());

            var sb = new StringBuilder();
            while (stack.Count > 0)
            {
                var currentTokenCount = GetTokenCount(stack.Peek() + splitter);
                var nextTokenCount = GetTokenCount(sb + stack.Peek() + splitter);

                if (currentTokenCount > ChunkSize)
                {
                    foreach (var chunk in ChunkContentRecursive(stack.Pop(), ChunkSplitters[splitter])) stack.Push(chunk);

                    continue;
                }

                if (currentTokenCount <= ChunkSize && nextTokenCount > ChunkSize)
                {
                    chunks.Add(sb.ToString().Trim('\n'));
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