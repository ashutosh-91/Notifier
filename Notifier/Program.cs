using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Notifier.Services;
using Notifier.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}