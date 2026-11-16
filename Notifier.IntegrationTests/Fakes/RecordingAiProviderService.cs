using Notifier.Services.Interfaces;

namespace Notifier.IntegrationTests.Fakes;

/// <summary>
/// Stands in for the real AI provider so integration tests never call out to a network.
/// </summary>
public sealed class RecordingAiProviderService : IAiProviderService
{
    public List<(string SystemPrompt, string UserPrompt)> Calls { get; } = [];

    public string? Result { get; set; } = "classified";

    public Exception? ThrowOnCall { get; set; }

    public Task<string?> CompleteChatAsync(string systemPrompt, string userPrompt)
    {
        Calls.Add((systemPrompt, userPrompt));
        return ThrowOnCall is null ? Task.FromResult(Result) : Task.FromException<string?>(ThrowOnCall);
    }
}
