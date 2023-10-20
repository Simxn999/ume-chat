using Azure;
using Azure.AI.OpenAI;
using Utilities;

namespace TitleGeneratorGPT;

/// <summary>
///     Title Generator utilizing ChatGPT.
/// </summary>
public static class TitleGenerator
{
    /// <summary>
    ///     Client handling Azure OpenAI.
    /// </summary>
    private static OpenAIClient? Client { get; set; }

    /// <summary>
    ///     Azure OpenAI GPT Deployment name.
    /// </summary>
    private static string? GPTDeployment { get; set; }

    /// <summary>
    ///     System message, title generation instructions.
    /// </summary>
    private static string? RoleInformation { get; set; }

    /// <summary>
    ///     If Title Generator is initialized or not.
    /// </summary>
    private static bool IsInitialized { get; set; }

    /// <summary>
    ///     Initialize Title Generator.
    /// </summary>
    public static void Initialize()
    {
        var endpoint = new Uri(Variables.Get("OPENAI_URL"));
        var keyCredential = new AzureKeyCredential(Variables.Get("OPENAI_API_KEY"));
        Client = new OpenAIClient(endpoint, keyCredential);
        GPTDeployment = Variables.Get("OPENAI_GPT_DEPLOYMENT");
        RoleInformation = Variables.Get("OPENAI_TITLE_GENERATOR_ROLEINFORMATION");

        IsInitialized = true;
    }

    /// <summary>
    ///     Generate Title based on given content.
    /// </summary>
    /// <param name="content">Content to base title on</param>
    /// <returns>String with new generated title</returns>
    public static async Task<string> GenerateTitleAsync(string content)
    {
        if (!IsInitialized)
            Initialize();

        var messages = GetMessages(content);
        return await GetChatResponse(messages);
    }

    /// <summary>
    ///     Retrieve the messages required for chat request.
    /// </summary>
    /// <param name="content">Content to base title on</param>
    /// <returns>Enumerable of ChatMessages used by GPT to generate title</returns>
    private static IEnumerable<ChatMessage> GetMessages(string content)
    {
        return new[] { new ChatMessage(ChatRole.System, RoleInformation), new ChatMessage(ChatRole.User, content) };
    }

    /// <summary>
    ///     Retrieve GPT response based on messages.
    /// </summary>
    /// <param name="messages">Messages to give GPT</param>
    /// <returns>String with GPT response</returns>
    private static async Task<string> GetChatResponse(IEnumerable<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(Client);

        var response = await Client.GetChatCompletionsAsync(GPTDeployment, new ChatCompletionsOptions(messages));
        return response.Value.Choices[0].Message.Content;
    }
}
