namespace Notifier.Services.Interfaces;

public interface IAiProviderService
{
    Task<string?> CompleteChatAsync(string systemPrompt, string userPrompt);
}
