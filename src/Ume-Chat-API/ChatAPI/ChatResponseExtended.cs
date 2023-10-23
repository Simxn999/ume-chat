using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Models.API.ChatAPI;

namespace ChatAPI;

/// <summary>
///     Extended version of ChatResponse.
/// </summary>
public partial class ChatResponseExtended : ChatResponse
{
    public ChatResponseExtended() { }

    /// <summary>
    ///     Initialize by ChatMessage.
    /// </summary>
    /// <param name="chatMessage">ChatMessage</param>
    public ChatResponseExtended(ChatMessage? chatMessage)
    {
        if (chatMessage is null)
            return;

        Content = GetMessage(chatMessage);
        Citations = GetCitations(chatMessage);
    }

    /// <summary>
    ///     Remove clutter from citations.
    /// </summary>
    public void DeclutterCitations()
    {
        RemoveUnusedCitations();
        CombineDuplicateDocumentIDs();
        RemoveDuplicateCitationsInMessage();
        NumberCitations();
    }

    /// <summary>
    ///     Retrieve the message content from a ChatMessage.
    /// </summary>
    /// <param name="message">ChatMessage</param>
    /// <returns>Content of ChatMessage</returns>
    private static string? GetMessage(ChatMessage message)
    {
        return !string.IsNullOrEmpty(message.Content) ? message.Content : null;
    }

    /// <summary>
    ///     Retrieve the citations from a ChatMessage.
    /// </summary>
    /// <param name="message">ChatMessage</param>
    /// <returns>Citations of ChatMessage</returns>
    private static List<Citation>? GetCitations(ChatMessage message)
    {
        var citationsString = message.AzureExtensionsContext?.Messages[0].Content;

        if (citationsString is null)
            return null;

        var responseCitations = JsonSerializer.Deserialize<ResponseCitations>(citationsString);

        return responseCitations?.Citations.Select((citation, i) => new Citation(i + 1, citation.Title, citation.URL)).ToList();
    }

    /// <summary>
    ///     Remove all citations that are not used in the message.
    /// </summary>
    private void RemoveUnusedCitations()
    {
        Citations?.RemoveAll(c => !Content?.Contains(c.TextID) ?? true);
    }

    /// <summary>
    ///     Combine document IDs that have the same URL.
    /// </summary>
    private void CombineDuplicateDocumentIDs()
    {
        if (Citations is null || Content is null)
            return;

        for (var i = Citations.Count - 1; i >= 0; i--)
        {
            var citation = Citations[i];

            // If current citation does not have a duplicate
            if (Citations.Count(c => c.URL == citation.URL) <= 1)
                continue;

            // Retrieve the first duplicate occurrence
            var firstOccuringDuplicate = Citations.FirstOrDefault(c => c.URL == citation.URL);

            if (firstOccuringDuplicate is null || firstOccuringDuplicate.TextID == citation.TextID)
                continue;

            // Replace current citation document ID with the first occuring duplicate document ID
            Content = Content.Replace(citation.TextID, firstOccuringDuplicate.TextID);

            // Remove current citation from list
            Citations.RemoveAt(i);
        }
    }

    /// <summary>
    ///     Remove duplicate occurrences of citations in message.
    /// </summary>
    private void RemoveDuplicateCitationsInMessage()
    {
        if (Content is null)
            return;

        Content = DocumentIDClusterRegex()
           .Replace(Content,
                    m =>
                    {
                        var seen = new HashSet<string>();
                        return DocumentIDRegex().Replace(m.Value, n => seen.Add(n.Value) ? n.Value : string.Empty);
                    });
    }

    /// <summary>
    ///     Populate citations with citation numbers based on the order that they occur in the message.
    /// </summary>
    private void NumberCitations()
    {
        if (Citations is null)
            return;

        // Retrieve all document IDs that occur in the message
        var matches = DocumentIDRegex().Matches(Content ?? string.Empty).DistinctBy(m => m.Value).ToList();

        for (var i = 0; i < matches.Count; i++)
        {
            var citation = Citations.FirstOrDefault(c => c.TextID == matches[i].Value);

            if (citation is null)
                continue;

            citation.CitationNumber = i + 1;
        }

        Citations.RemoveAll(c => c.CitationNumber == -1);
    }

    /// <summary>
    ///     <para>Regular expression matching all document references.</para>
    ///     <para>Example: [doc1], [doc2], [doc3]</para>
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex(@"\[doc(\d+)\]")]
    private static partial Regex DocumentIDRegex();

    /// <summary>
    ///     <para>Regular expression matching citation clusters.</para>
    ///     <para>Example: [doc1][doc2][doc3], [doc1][doc3], [doc1]</para>
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex(@"((?:\[(doc\d+)\])+)")]
    private static partial Regex DocumentIDClusterRegex();
}
