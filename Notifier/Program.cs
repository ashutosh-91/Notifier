using System.Net.Http.Headers;
using System.Text.Json.Serialization;
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

builder.Services.AddSingleton(new RollingWindowRateLimiter(limit: 10, window: TimeSpan.FromMinutes(1)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapControllers();
app.Run();


public partial class Program { }