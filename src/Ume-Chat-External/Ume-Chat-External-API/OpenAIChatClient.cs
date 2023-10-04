using System.Globalization;
using Azure;
using Azure.AI.OpenAI;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.API.Request;

namespace Ume_Chat_External_API;

/// <summary>
///     Client for handling chat requests to Azure OpenAI.
/// </summary>
public static class OpenAIChatClient
{
    /// <summary>
    ///     Endpoint to Azure OpenAI.
    /// </summary>
    private static Uri OpenAIEndpoint { get; } = new Uri(Variables.Get("OPENAI_URL"));

    /// <summary>
    ///     Key to Azure OpenAI.
    /// </summary>
    private static AzureKeyCredential OpenAIKey { get; } = new AzureKeyCredential(Variables.Get("OPENAI_API_KEY"));

    /// <summary>
    ///     Name of Azure OpenAI ChatGPT deployment.
    /// </summary>
    private static string GPTDeployment { get; } = Variables.Get("OPENAI_GPT_DEPLOYMENT");

    /// <summary>
    ///     ChatGPT system message.
    ///     In other words, assistant instructions.
    /// </summary>
    private static string RoleInformation { get; } = Variables.Get("API_CHAT_REQUEST_ROLE_INFORMATION");

    /// <summary>
    ///     Parameter of creativity and randomness in chat responses.
    ///     Ranges 0.0-2.0, recommended 0.0-1.0
    ///     0.0 = Consistent, deterministic and focused responses. In other words, autistic.
    ///     1.0 = Diverse and creative responses.
    ///     2.0 = Absolute gibberish
    /// </summary>
    private static float Temperature { get; } = Variables.GetFloat("API_CHAT_REQUEST_TEMPERATURE");

    /// <summary>
    ///     Maximum amount of tokens allowed per call.
    /// </summary>
    private static int MaxTokens { get; } = Variables.GetInt("API_CHAT_REQUEST_MAX_TOKENS");

    /// <summary>
    ///     Amount of documents sent to the chatbot.
    /// </summary>
    private static int DocumentCount { get; } = Variables.GetInt("API_CHAT_REQUEST_DOCUMENT_COUNT");

    /// <summary>
    ///     Endpoint to Azure OpenAI Embedding deployment.
    /// </summary>
    private static Uri EmbeddingEndpoint { get; } = new Uri(Variables.Get("OPENAI_EMBEDDING_ENDPOINT"));

    /// <summary>
    ///     Name of Azure Cognitive Search Index. Also known as the database.
    /// </summary>
    private static string IndexName { get; } = Variables.Get("COGNITIVE_SEARCH_INDEX_NAME");

    /// <summary>
    ///     Endpoint to Azure Cognitive Search.
    /// </summary>
    private static Uri SearchEndpoint { get; } = new Uri(Variables.Get("COGNITIVE_SEARCH_URL"));

    /// <summary>
    ///     Key to Azure Cognitive Search.
    /// </summary>
    private static AzureKeyCredential SearchKey { get; } =
        new AzureKeyCredential(Variables.Get("COGNITIVESEARCH_API_KEY"));

    /// <summary>
    ///     Key to Azure Cognitive Search.
    /// </summary>
    private static string SemanticConfig { get; } = Variables.Get("API_CHAT_REQUEST_SEMANTIC_CONFIG");

    /// <summary>
    ///     Name of content field.
    /// </summary>
    private static string ContentField { get; } = Variables.Get("API_CHAT_REQUEST_CONTENT_FIELD");

    /// <summary>
    ///     Separator pattern that content fields should use.
    /// </summary>
    private static string ContentFieldSeparator { get; } = Variables.Get("API_CHAT_REQUEST_CONTENT_SEPARATOR");

    /// <summary>
    ///     Name of title field.
    /// </summary>
    private static string TitleField { get; } = Variables.Get("API_CHAT_REQUEST_TITLE_FIELD");

    /// <summary>
    ///     Name of URL field.
    /// </summary>
    private static string URLField { get; } = Variables.Get("API_CHAT_REQUEST_URL_FIELD");

    /// <summary>
    ///     Name of vector field.
    /// </summary>
    private static string VectorField { get; } = Variables.Get("API_CHAT_REQUEST_VECTOR_FIELD");

    /// <summary>
    ///     Client for handling requests to Azure OpenAI.
    /// </summary>
    private static OpenAIClient Client { get; } = new OpenAIClient(OpenAIEndpoint, OpenAIKey);

    /// <summary>
    ///     Configured AzureChatExtensionsOptions.
    /// </summary>
    private static AzureChatExtensionsOptions AzureExtensionsOptions { get; } = GetAzureExtensionsOptions();

    /// <summary>
    ///     Type of query for chatbot to use in data retrieval.
    /// </summary>
    private static AzureCognitiveSearchQueryType QueryType => AzureCognitiveSearchQueryType.VectorSemanticHybrid;

    /// <summary>
    ///     Request a chat response from ChatGPT based on the messages provided.
    /// </summary>
    /// <param name="chatMessages">Messages</param>
    /// <returns>Answer message and citations</returns>
    public static async Task<ChatResponseExtended> SendChatRequestAsync(IEnumerable<ChatMessage> chatMessages)
    {
        var response = await Client.GetChatCompletionsAsync(GPTDeployment, GetChatCompletionOptions(chatMessages));
        var message = response.Value?.Choices[0].Message;

        return new ChatResponseExtended(message);
    }

    /// <summary>
    ///     Request a streaming chat response from ChatGPT based on the messages provided.
    /// </summary>
    /// <param name="chatMessages">Messages</param>
    /// <returns>Answer message and citations chunked in an asynchronous enumerable</returns>
    public static async Task<IAsyncEnumerable<ChatMessage>> SendChatRequestStreamingAsync(
        IEnumerable<ChatMessage> chatMessages)
    {
        var response =
            await Client.GetChatCompletionsStreamingAsync(GPTDeployment, GetChatCompletionOptions(chatMessages));

        var asyncEnumerator = response.Value.GetChoicesStreaming().GetAsyncEnumerator();

        await asyncEnumerator.MoveNextAsync();

        return asyncEnumerator.Current.GetMessageStreaming();
    }

    /// <summary>
    ///     Parse request input to proper object and populate with system message.
    /// </summary>
    /// <param name="requestMessages">Messages</param>
    /// <returns>Enumerable of messages including system message</returns>
    public static IEnumerable<ChatMessage> GetChatMessages(IEnumerable<RequestMessage> requestMessages)
    {
        var date = DateTime.Now;
        var cultureInfo = new CultureInfo("sv-SE");
        var messages = new List<ChatMessage>
                       {
                           new ChatMessage(ChatRole.System, RoleInformation),
                           new ChatMessage(ChatRole.User,
                                           $"Dagens datum är {date.ToString("D", cultureInfo)} och klockan är just nu {date:t}.")
                       };

        messages.AddRange(requestMessages.Select(m => new ChatMessage(GetChatRole(m.Role), m.Message)));

        return messages;
    }

    /// <summary>
    ///     Convert role as string to proper ChatRole object.
    /// </summary>
    /// <param name="role">Role as string</param>
    /// <returns>Role as ChatRole</returns>
    /// <exception cref="Exception">Invalid role!</exception>
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

    /// <summary>
    ///     Retrieve configured ChatCompletionsOptions.
    /// </summary>
    /// <param name="chatMessages">Messages</param>
    /// <returns>Configured ChatCompletionsOptions</returns>
    private static ChatCompletionsOptions GetChatCompletionOptions(IEnumerable<ChatMessage> chatMessages)
    {
        return new ChatCompletionsOptions(chatMessages)
               {
                   // MaxTokens = MaxTokens,
                   Temperature = Temperature,
                   AzureExtensionsOptions = AzureExtensionsOptions
               };
    }

    /// <summary>
    ///     Retrieve configured AzureChatExtensionsOptions.
    /// </summary>
    /// <returns>Configured AzureChatExtensionsOptions</returns>
    private static AzureChatExtensionsOptions GetAzureExtensionsOptions()
    {
        return new AzureChatExtensionsOptions { Extensions = { GetAzureCognitiveSearchChatExtensionConfiguration() } };
    }

    /// <summary>
    ///     Retrieve configured AzureCognitiveSearchChatExtensionConfiguration.
    /// </summary>
    /// <returns>Configured AzureCognitiveSearchChatExtensionConfiguration</returns>
    private static AzureCognitiveSearchChatExtensionConfiguration GetAzureCognitiveSearchChatExtensionConfiguration()
    {
        return new AzureCognitiveSearchChatExtensionConfiguration
               {
                   DocumentCount = DocumentCount,
                   EmbeddingEndpoint = EmbeddingEndpoint,
                   EmbeddingKey = OpenAIKey,
                   FieldMappingOptions = GetFieldMappingOptions(),
                   IndexName = IndexName,
                   QueryType = QueryType,
                   SearchEndpoint = SearchEndpoint,
                   SearchKey = SearchKey,
                   ShouldRestrictResultScope = true,
                   Type = AzureChatExtensionType.AzureCognitiveSearch,
                   SemanticConfiguration = SemanticConfig
               };
    }

    /// <summary>
    ///     Retrieve configured AzureCognitiveSearchIndexFieldMappingOptions.
    /// </summary>
    /// <returns>Configured AzureCognitiveSearchIndexFieldMappingOptions</returns>
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
