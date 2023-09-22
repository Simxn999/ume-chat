using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Azure.AI.OpenAI;
using Ume_Chat_External_General.Models.API.Request;
using Ume_Chat_External_General.Models.API.Response;

namespace Ume_Chat_External_API;

public static class DataManager
{
    public static async Task<IResult> ChatAsync(HttpContext context, List<RequestMessage> messages, bool stream)
    {
        try
        {
            var chatMessages = OpenAIChatClient.GetChatMessages(messages);

            if (!stream)
                return Results.Ok(await GetChatResponseAsync(chatMessages));

            await GetChatResponseStreamingAsync(context, chatMessages);

            return Results.Empty;
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    private static async Task<ChatResponse> GetChatResponseAsync(IEnumerable<ChatMessage> messages)
    {
        var chatResponse = await OpenAIChatClient.SendChatRequestAsync(messages);
        chatResponse.RemoveUnusedCitations();
        chatResponse.NumberCitations();

        return chatResponse;
    }

    private static async Task GetChatResponseStreamingAsync(HttpContext context, IEnumerable<ChatMessage> messages)
    {
        context.Response.Headers.Add("Content-Type", "text/event-stream");

        var chunks = await OpenAIChatClient.SendChatRequestStreamingAsync(messages);
        var completeChatResponse = new ChatResponseExtended();
        await using var writer = new StreamWriter(context.Response.Body);

        await foreach (var chunk in chunks)
        {
            var chunkChatResponse = await WriteChunkToStreamAsync(chunk, writer);

            if (chunkChatResponse.Citations is not null)
                completeChatResponse.Citations?.AddRange(chunkChatResponse.Citations);

            if (chunkChatResponse.Message is not null)
                completeChatResponse.Message += chunkChatResponse.Message;
        }

        await WriteEndToStreamAsync(writer);

        completeChatResponse.RemoveUnusedCitations();
        completeChatResponse.NumberCitations();

        await WriteObjectToStreamAsync(completeChatResponse, writer);
    }

    private static async Task<ChatResponse> WriteChunkToStreamAsync(ChatMessage chunk, TextWriter writer)
    {
        var chatResponse = new ChatResponseExtended(chunk);

        if (chatResponse.Message is null && chatResponse.Citations is null)
            return chatResponse;

        await WriteObjectToStreamAsync(chatResponse, writer);

        return chatResponse;
    }

    private static async Task WriteObjectToStreamAsync(object @object, TextWriter writer)
    {
        var jsonOptions = new JsonSerializerOptions
                          {
                              Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                          };
        var jsonString = JsonSerializer.Serialize(@object, jsonOptions);

        await writer.WriteAsync(GetDataString(jsonString));
        await writer.FlushAsync();
    }

    private static string GetDataString(string content)
    {
        return $"data: {content}\n\n";
    }

    private static async Task WriteEndToStreamAsync(TextWriter writer)
    {
        await writer.WriteAsync(GetDataString("[DONE]"));
        await writer.FlushAsync();
    }
}