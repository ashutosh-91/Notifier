using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notifier.IntegrationTests.Fakes;
using Notifier.Services;
using Notifier.Services.Interfaces;

namespace Notifier.IntegrationTests;

/// <summary>
/// Boots the real API in-memory and replaces the external AI and Discord boundaries with recording stubs.
/// </summary>
public sealed class NotifierApiFactory : WebApplicationFactory<Program>
{
    public RecordingAiProviderService Ai { get; } = new();

    public RecordingDiscordClientService Discord { get; } = new();

    /// <summary>
    /// Tokens available with no refill. Default is high so only the rate-limit test is constrained.
    /// Set before the host starts.
    /// </summary>
    public int RateLimitTokens { get; init; } = 1000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ai:BaseUrl"] = "https://ai.example.test",
                ["Ai:Model"] = "test-model",
                ["Ai:ApiKey"] = "test-key",
                ["Ai:SystemPrompts:NotificationClassifier"] = "classify",
                ["Discord:BotToken"] = "test-token",
                ["Discord:ChannelId"] = "42"
            }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAiProviderService>();
            services.AddSingleton<IAiProviderService>(Ai);

            services.RemoveAll<IDiscordClientService>();
            services.AddSingleton<IDiscordClientService>(Discord);

            services.RemoveAll<RollingWindowRateLimiter>();
            services.AddSingleton(new RollingWindowRateLimiter(limit: RateLimitTokens, window: TimeSpan.FromMinutes(1)));
        });
    }
}
