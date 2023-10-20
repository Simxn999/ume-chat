using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Azure.AI.OpenAI;
using Models.API.ChatAPI;

namespace ChatAPI;

/// <summary>
///     Data manager for API requests.
/// </summary>
public static class DataManager
{
    /// <summary>
    ///     Request a response from the assistant based on the messages provided.
    /// </summary>
    /// <param name="context">HttpContext used for streaming the response</param>
    /// <param name="messages">Messages</param>
    /// <param name="stream">If response should be streaming or not</param>
    /// <returns>Assistant answer with citations</returns>
    public static async Task<IResult> ChatAsync(HttpContext context, List<RequestMessage> messages, bool stream)
    {
        try
        {
            // Parse request input and populate with system message
            var chatMessages = ChatClient.GetChatMessages(messages);

            if (!stream)
                // Not streaming response
                return Results.Ok(await GetChatResponseAsync(chatMessages));

            // Streaming response
            await GetChatResponseStreamingAsync(context, chatMessages);

            return Results.Empty;
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    /// <summary>
    ///     Request a response from the assistant based on the messages provided.
    /// </summary>
    /// <param name="messages">Messages</param>
    /// <returns>Assistant answer with citations</returns>
    private static async Task<ChatResponse> GetChatResponseAsync(IEnumerable<ChatMessage> messages)
    {
        var chatResponse = await ChatClient.SendChatRequestAsync(messages);
        chatResponse.DeclutterCitations();

        return chatResponse;
    }

    /// <summary>
    ///     Request a streaming response from the assistant based on the messages provided.
    /// </summary>
    /// <param name="context">HttpContext used for streaming the response</param>
    /// <param name="messages">Messages</param>
    private static async Task GetChatResponseStreamingAsync(HttpContext context, IEnumerable<ChatMessage> messages)
    {
        context.Response.Headers["Content-Type"] = "text/event-stream";

        var chunks = await ChatClient.SendChatRequestStreamingAsync(messages);
        await using var writer = new StreamWriter(context.Response.Body);
        var completeChatResponse = new ChatResponseExtended();
        var jsonOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

        // Writes chunks to response & compiles a complete response object
        await foreach (var chunk in chunks)
        {
            // Write chunk to response
            var chunkChatResponse = await WriteChunkToStreamAsync(chunk, writer, jsonOptions);

            // Compile complete chat response

            if (chunkChatResponse.Citations is not null)
                completeChatResponse.Citations = chunkChatResponse.Citations;

            if (chunkChatResponse.Message is not null)
                completeChatResponse.Message += chunkChatResponse.Message;
        }

        // Indicate end of stream
        await WriteEndToStreamAsync(writer);

        // Declutter compiled chat response
        completeChatResponse.DeclutterCitations();

        // Write compiled chat response
        await WriteObjectToStreamAsync(completeChatResponse, writer, jsonOptions);
    }

    /// <summary>
    ///     Writes a message chunk to the response.
    /// </summary>
    /// <param name="chunk">Message to write</param>
    /// <param name="writer">Writer</param>
    /// <param name="jsonOptions">JsonSerializerOptions</param>
    /// <returns>Assistant answer chunk with citations</returns>
    private static async Task<ChatResponse> WriteChunkToStreamAsync(ChatMessage chunk, TextWriter writer, JsonSerializerOptions jsonOptions)
    {
        var chatResponse = new ChatResponseExtended(chunk);

        if (chatResponse.Message is null && chatResponse.Citations is null)
            return chatResponse;

        await WriteObjectToStreamAsync(chatResponse, writer, jsonOptions);

        return chatResponse;
    }

    /// <summary>
    ///     Writes an object to the response.
    /// </summary>
    /// <param name="object">Object to write</param>
    /// <param name="writer">Writer</param>
    /// <param name="jsonOptions">JsonSerializerOptions</param>
    private static async Task WriteObjectToStreamAsync(object @object, TextWriter writer, JsonSerializerOptions jsonOptions)
    {
        var jsonString = JsonSerializer.Serialize(@object, jsonOptions);

        await writer.WriteAsync(GetDataString(jsonString));
        await writer.FlushAsync();
    }

    /// <summary>
    ///     Retrieve properly formated datastream string.
    /// </summary>
    /// <param name="content">Data content</param>
    /// <returns>Properly formatted datastream</returns>
    private static string GetDataString(string content)
    {
        return $"data: {content}\n\n";
    }

    /// <summary>
    ///     Write to response that the stream is finished.
    /// </summary>
    /// <param name="writer">Writer</param>
    private static async Task WriteEndToStreamAsync(TextWriter writer)
    {
        await writer.WriteAsync(GetDataString("[DONE]"));
        await writer.FlushAsync();
    }
}
