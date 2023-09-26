using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Ume_Chat_External_General.Models.API.Response;

namespace Ume_Chat_External_API;

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

        Message = GetMessage(chatMessage);
        Citations = GetCitations(chatMessage);
    }

    /// <summary>
    ///     Remove all citations that are not used in the message.
    /// </summary>
    public void RemoveUnusedCitations()
    {
        Citations?.RemoveAll(c => !Message?.Contains(c.DocumentID) ?? true);
    }

    /// <summary>
    ///     Populate citations with citation numbers based on the order that they appear in the message.
    /// </summary>
    public void NumberCitations()
    {
        var matches = DocumentIDRegex().Matches(Message ?? "").DistinctBy(m => m.Value).ToList();

        for (var i = 0; i < matches.Count; i++)
        {
            var citation = Citations?.FirstOrDefault(c => c.DocumentID == matches[i].Value);

            if (citation is null)
                continue;

            citation.CitationNumber = i + 1;
        }

        Citations?.RemoveAll(c => c.CitationNumber == -1);
    }

    /// <summary>
    ///     Regular expression matching all document references in a message.
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex(@"\[doc(\d+)\]")]
    private static partial Regex DocumentIDRegex();

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
        var output = new List<Citation>();

        var citationsString = message.AzureExtensionsContext?.Messages[0].Content;

        if (citationsString is null)
            return null;

        var responseCitations = JsonSerializer.Deserialize<ResponseCitations>(citationsString);

        for (var i = 0; i < responseCitations?.Citations.Count; i++)
        {
            var citation = responseCitations.Citations[i];
            output.Add(new Citation(i + 1, citation.Title, citation.URL));
        }

        return output;
    }
}
