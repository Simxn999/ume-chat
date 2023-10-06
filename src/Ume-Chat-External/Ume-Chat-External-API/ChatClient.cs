using Ume_Chat_External_API.Models;
using Ume_Chat_External_General;
using Ume_Chat_External_General.Models.API.Request;

namespace Ume_Chat_External_API;

/// <summary>
///     Client for handling chat requests to Azure OpenAI.
/// </summary>
public static class ChatClient
{
    /// <summary>
    ///     Endpoint to Azure OpenAI Chat.
    /// </summary>
    private static string AzureChatEndpoint { get; } = Variables.Get("AZURE_CHAT_ENDPOINT");

    /// <summary>
    ///     Key to Azure OpenAI.
    /// </summary>
    private static string OpenAIKey { get; } = Variables.Get("OPENAI_API_KEY");

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
    ///     If chat answers should be restricted to provided documents or not.
    /// </summary>
    private static bool InScope { get; } = Variables.GetBool("API_CHAT_REQUEST_IN_SCOPE");
    
    /// <summary>
    ///     Parameter of creativity and randomness in chat responses.
    ///     Ranges 0.0-2.0, recommended 0.0-1.0
    ///     0.0 = Consistent, deterministic and focused responses. In other words, autistic.
    ///     1.0 = Diverse and creative responses.
    ///     2.0 = Absolute gibberish
    /// </summary>
    private static float Temperature { get; } = Variables.GetFloat("API_CHAT_REQUEST_TEMPERATURE");

    /// <summary>
    ///     Type of DataSource in request.
    /// </summary>
    private static string DataSourceType { get; } = Variables.Get("API_CHAT_REQUEST_TYPE");
    
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
    private static string EmbeddingEndpoint { get; } = Variables.Get("OPENAI_EMBEDDING_ENDPOINT");

    /// <summary>
    ///     Name of Azure Cognitive Search Index. Also known as the database.
    /// </summary>
    private static string IndexName { get; } = Variables.Get("COGNITIVE_SEARCH_INDEX_NAME");

    /// <summary>
    ///     Endpoint to Azure Cognitive Search.
    /// </summary>
    private static string SearchEndpoint { get; } = Variables.Get("COGNITIVE_SEARCH_URL");

    /// <summary>
    ///     Key to Azure Cognitive Search.
    /// </summary>
    private static string SearchKey { get; } = Variables.Get("COGNITIVESEARCH_API_KEY");

    /// <summary>
    ///     Key to Azure Cognitive Search.
    /// </summary>
    private static string SemanticConfig { get; } = Variables.Get("API_CHAT_REQUEST_SEMANTIC_CONFIG");

    /// <summary>
    ///     Name of content field.
    /// </summary>
    private static string ContentField { get; } = Variables.Get("API_CHAT_REQUEST_CONTENT_FIELD");

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
    ///     Type of query for chatbot to use in data retrieval.
    /// </summary>
    private static string QueryType => Variables.Get("API_CHAT_REQUEST_QUERY_TYPE");

    private static DataSource DefaultDataSource { get; } = GetDataSource();

    /// <summary>
    ///     Request a chat response from ChatGPT based on the messages provided.
    /// </summary>
    /// <param name="messages">Messages</param>
    /// <returns>Answer message and citations</returns>
    public static async Task<ChatResponseExtended> SendChatRequestAsync(IEnumerable<RequestMessage> messages)
    {
        await GetChatCompletionsAsync(messages);

        return new ChatResponseExtended();
    }

    private static async Task GetChatCompletionsAsync(IEnumerable<RequestMessage> messages)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", OpenAIKey);

        var requestBody = GetRequestBody(messages, false);

        var jsonContent = JsonContent.Create(requestBody);

        // var request = new HttpRequestMessage(HttpMethod.Post, AzureChatEndpoint);
        // request.Headers.Add("api-key", OpenAIKey);
        // request.Content = jsonContent;
        //
        // var result = await httpClient.SendAsync(request);

        var result = await httpClient.PostAsync(AzureChatEndpoint, jsonContent);
    }

    private static RequestBody GetRequestBody(IEnumerable<RequestMessage> messages, bool stream)
    {
        return new RequestBody
               {
                   Messages = messages,
                   Temperature = Temperature,
                   Stream = stream,
                   DataSources = new[] { DefaultDataSource }
               };
    }

    private static DataSource GetDataSource()
    {
        return new DataSource
               {
                   Type = DataSourceType,
                   Parameters = GetParameters()
               };
    }

    private static Parameters GetParameters()
    {
        return new Parameters
               {
                   Endpoint = SearchEndpoint,
                   Key = SearchKey,
                   IndexName = IndexName,
                   QueryType = QueryType,
                   SemanticConfiguration = SemanticConfig,
                   EmbeddingEndpoint = EmbeddingEndpoint,
                   EmbeddingKey = OpenAIKey,
                   RoleInformation = GetRoleInformation(),
                   InScope = InScope,
                   FieldsMapping = GetFieldsMapping()
               };
    }

    private static string GetRoleInformation()
    {
        var date = DateTime.Now;
        return RoleInformation + $"\n\n\nToday's date is {date:D}.\nThe clock is {date:t}.";
    }

    private static FieldsMapping GetFieldsMapping()
    {
        return new FieldsMapping
               {
                   TitleField = TitleField,
                   URLField = URLField,
                   ContentFields = new[] { ContentField },
                   VectorFields = new[] { VectorField }
               };
    }
}
