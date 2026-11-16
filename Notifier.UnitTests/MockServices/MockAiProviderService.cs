using Notifier.Services.Interfaces;

namespace Notifier.UnitTests.Fakes;

public sealed class MockAiProviderService : IAiProviderService
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
