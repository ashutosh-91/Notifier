# Notifier 

An ASP.NET Core service that receives notifications over HTTP and forwards an LLM-written message to
Discord when the notification is a warning or worse.

# Environment 
- .NET 10 

## How it works

![Diagram](https://pub-2525f5c4eebc479ca7a4926bebc7a6c6.r2.dev/Screenshot%202026-11-16%20at%206.31.14%E2%80%AFPM.png)

- Notification levels: `Info`, `Warning`, `Error`, `Critical` (forwarded when level ≥ `Warning`).

## Project layout

 - Notifier The API (controllers, services, models)
 - Notifier.UnitTests
 - Notifier.IntegrationTests

# Architecture
- The code organised into separate layers to keep things simple, maintainable, and easy to extend.

- Controllers: Handle incoming HTTP requests, call the appropriate service, and return the response with the correct status code.

- Services: NotificationClassifierService handles the main application logic. It checks the rate limit, classifies the notification using AI, and sends it to Discord if the severity is Warning or higher.

- Models: Contains the data structures used throughout the application, such as Notification.

- Interfaces: Define how the application communicates with external services like OpenAI and Discord, without depending on their actual implementations.

- External Services: AiProviderService and DiscordClientService handle the actual API communication with OpenAI and Discord.

## Configuration

Settings are read from the standard configuration sources (`appsettings.json`, `appsettings.{Environment}.json`,
environment variables, user-secrets). Keys:

| Key | Description |
| --- | --- |
| `Ai:BaseUrl` | OpenAI compatible endpoint, e.g. `https://api.deepseek.com` |
| `Ai:Model` | Model name, e.g. `deepseek-chat` |
| `Ai:ApiKey` | API key for the LLM provider |
| `Ai:SystemPrompts:NotificationClassifier` | System prompt used to classify and write the message |
| `Discord:BotToken` | Discord bot token |
| `Discord:ChannelId` | Target Discord channel id |


## API

### `POST /api/v1/notifications/receive`

Request body:

```json
{
  "id": 1,
  "level": "Warning",
  "message": "Payment failure detected"
}
```

These are my current assumption I took for the "Levels" -  `Info`, `Warning`, `Error`, `Critical`.

Responses:

| Status | Meaning | Body |
| --- | --- | --- |
| `200 OK` | Below the threshold — nothing forwarded | `{ "status": "ignored" }` |
| `202 Accepted` | Classified and forwarded | `{ "status": "forwarded", "message": "..." }` |
| `400 Bad Request` | Invalid payload (e.g. empty `message`) | ProblemDetails |
| `429 Too Many Requests` | Rate limit reached | ProblemDetails + `Retry-After` header |
| `502 Bad Gateway` | The AI provider or Discord is unavailable | ProblemDetails |


## Rate limiting

  Rolling Window Limiter was used in this project we remove any send on the list that happened more than a minute ago if 10 times are still left it does not let the request process.

## Testing

- **Unit tests** cover the level threshold, the outcome for each path (forwarded / rate-limited / dependency
  failure), the rate limiter, and the Discord request shape.
- **Integration tests** boot the real app in memory (`WebApplicationFactory`) with the AI and Discord
  dependencies replaced by recording stubs, and assert on HTTP status codes.


 ## What can be done better?
 - Right now the requests can be duplicated that is Request 1 (Id : 100) Request 2 (Id : 100) and we still process it. This can be prevented using a persistent layer such as DB to store the Idempotent keys this would prevent duplicate LLM calls as well.
 - Instead of just rate limitting adding a retry mechanism for failures.
 - Add Logging at each level.