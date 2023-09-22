using Azure;
using Azure.AI.OpenAI;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.API.Request;

namespace Ume_Chat_External_API;

public static class OpenAIChatClient
{
    private static Uri OpenAIEndpoint { get; } = new Uri(Variables.Get("OPENAI_URL"));
    private static AzureKeyCredential OpenAIKey { get; } = new AzureKeyCredential(Variables.Get("OPENAI_API_KEY"));
    private static string GPTDeployment { get; } = Variables.Get("OPENAI_GPT_DEPLOYMENT");
    private static string RoleInformation { get; } = Variables.Get("API_CHAT_REQUEST_ROLE_INFORMATION");
    private static float Temperature { get; } = float.Parse(Variables.Get("API_CHAT_REQUEST_TEMPERATURE"));
    private static int MaxTokens { get; } = int.Parse(Variables.Get("API_CHAT_REQUEST_MAX_TOKENS"));
    private static Uri EmbeddingEndpoint { get; } = new Uri(Variables.Get("OPENAI_EMBEDDING_ENDPOINT"));
    private static string IndexName { get; } = Variables.Get("COGNITIVE_SEARCH_INDEX_NAME");
    private static Uri SearchEndpoint { get; } = new Uri(Variables.Get("COGNITIVE_SEARCH_URL"));
    private static AzureKeyCredential SearchKey { get; } = new AzureKeyCredential(Variables.Get("COGNITIVESEARCH_API_KEY"));
    private static string ContentField { get; } = Variables.Get("API_CHAT_REQUEST_CONTENT_FIELD");
    private static string ContentFieldSeparator { get; } = Variables.Get("API_CHAT_REQUEST_CONTENT_SEPARATOR");
    private static string TitleField { get; } = Variables.Get("API_CHAT_REQUEST_TITLE_FIELD");
    private static string URLField { get; } = Variables.Get("API_CHAT_REQUEST_URL_FIELD");
    private static string VectorField { get; } = Variables.Get("API_CHAT_REQUEST_VECTOR_FIELD");
    private static OpenAIClient Client { get; } = new OpenAIClient(OpenAIEndpoint, OpenAIKey);
    private static AzureChatExtensionsOptions AzureExtensionsOptions { get; } = GetAzureExtensionsOptions();
    private static AzureCognitiveSearchQueryType QueryType => AzureCognitiveSearchQueryType.VectorSimpleHybrid;

    public static async Task<ChatResponseExtended> SendChatRequestAsync(IEnumerable<ChatMessage> chatMessages)
    {
        var response = await Client.GetChatCompletionsAsync(GPTDeployment, GetChatCompletionOptions(chatMessages));
        var message = response.Value?.Choices[0].Message;

        return new ChatResponseExtended(message);
    }

    public static async Task<IAsyncEnumerable<ChatMessage>> SendChatRequestStreamingAsync(IEnumerable<ChatMessage> chatMessages)
    {
        var response = await Client.GetChatCompletionsStreamingAsync(GPTDeployment, GetChatCompletionOptions(chatMessages));

        var asyncEnumerator = response.Value.GetChoicesStreaming().GetAsyncEnumerator();

        await asyncEnumerator.MoveNextAsync();

        return asyncEnumerator.Current.GetMessageStreaming();
    }

    public static IEnumerable<ChatMessage> GetChatMessages(IEnumerable<RequestMessage> requestMessages)
    {
        var messages = new List<ChatMessage>
                       {
                           new ChatMessage(ChatRole.System, RoleInformation)
                       };

        messages.AddRange(requestMessages.Select(m => new ChatMessage(GetChatRole(m.Role), m.Message)));

        return messages;
    }

    private static ChatRole GetChatRole(string role)
    {
        return role.ToLower() switch
               {
                   "user" => ChatRole.User,
                   "assistant" => ChatRole.Assistant,
                   "system" => ChatRole.System,
                   "tool" => ChatRole.Tool,
                   "function" => ChatRole.Function,
                   _ => throw new Exception("Invalid role!")
               };
    }

    private static ChatCompletionsOptions GetChatCompletionOptions(IEnumerable<ChatMessage> chatMessages)
    {
        return new ChatCompletionsOptions(chatMessages)
               {
                   // MaxTokens = MaxTokens,
                   Temperature = Temperature,
                   AzureExtensionsOptions = AzureExtensionsOptions
               };
    }

    private static AzureChatExtensionsOptions GetAzureExtensionsOptions()
    {
        return new AzureChatExtensionsOptions
               {
                   Extensions =
                   {
                       GetAzureCognitiveSearchChatExtensionConfiguration()
                   }
               };
    }

    private static AzureCognitiveSearchChatExtensionConfiguration GetAzureCognitiveSearchChatExtensionConfiguration()
    {
        return new AzureCognitiveSearchChatExtensionConfiguration
               {
                   EmbeddingEndpoint = EmbeddingEndpoint,
                   EmbeddingKey = OpenAIKey,
                   FieldMappingOptions = GetFieldMappingOptions(),
                   IndexName = IndexName,
                   QueryType = QueryType,
                   SearchEndpoint = SearchEndpoint,
                   SearchKey = SearchKey,
                   ShouldRestrictResultScope = true,
                   Type = AzureChatExtensionType.AzureCognitiveSearch
               };
    }

    private static AzureCognitiveSearchIndexFieldMappingOptions GetFieldMappingOptions()
    {
        return new AzureCognitiveSearchIndexFieldMappingOptions
               {
                   ContentFieldNames = { ContentField },
                   ContentFieldSeparator = ContentFieldSeparator,
                   TitleFieldName = TitleField,
                   UrlFieldName = URLField,
                   VectorFieldNames = { VectorField }
               };
    }
}