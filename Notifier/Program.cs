using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Notifier.Models;
using Notifier.Services;
using Notifier.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddScoped<NotificationClassifierService>();
builder.Services.AddScoped<IAiProviderService, AiProviderService>();
builder.Services.AddHttpClient<IDiscordClientService, DiscordClientService>( "Discord",(client) =>
{
    client.BaseAddress = new Uri("https://discord.com/api/v10/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", builder.Configuration["Discord:BotToken"]);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddOptions<Ai>()
    .Bind(builder.Configuration.GetSection("Ai"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Cap outbound notifications at 10/minute.
builder.Services.AddSingleton<RateLimiter>(_ => new TokenBucketRateLimiter(
    new TokenBucketRateLimiterOptions
    {
        TokenLimit = 10,
        TokensPerPeriod = 10,
        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
        AutoReplenishment = true,
        QueueLimit = 0
    }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapControllers();
app.Run();


public partial class Program { }