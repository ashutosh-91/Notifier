using Notifier.Services.Interfaces;

namespace Notifier.UnitTests.Fakes;

public sealed class FakeAiProviderService : IAiProviderService
{
    public List<(string SystemPrompt, string UserPrompt)> Calls { get; } = [];

    public string? Result { get; set; } = "classified";

    public Task<string?> CompleteChatAsync(string systemPrompt, string userPrompt)
    {
        Calls.Add((systemPrompt, userPrompt));
        return Task.FromResult(Result);
    }
}
