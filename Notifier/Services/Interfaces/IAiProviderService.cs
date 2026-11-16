namespace Notifier.Services.Interfaces;

public interface IAiProviderService
{
    /// <summary>
    /// Calls the LLM with OpenAI Sdk for chat completion.
    /// </summary>
    /// <param name="systemPrompt">LLM role/guardrails and information for the assitant.</param>
    /// <param name="userPrompt">Users message.</param>
    /// <returns></returns>
    Task<string?> CompleteChatAsync(string systemPrompt, string userPrompt);
}
