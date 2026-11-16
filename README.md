# Notifier 

A ASP.NET Core service that receives notifications over HTTP and forwards an LLM-written message to
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
 - It is divided into Models , Controllers , Services 
 - Notifier is the main application , Notifier.UnitTests and IntegrationTests hosts the testing.

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

Error responses use the standard `application/problem+json` (RFC 9457) shape.

## Rate limiting

I've used a Rolling Window Limiter in this we remove any send on the list that happened more than a minute ago if 10 times are still left it does not let the request process.

## Testing

- **Unit tests** cover the level threshold, the outcome for each path (forwarded / rate-limited / dependency
  failure), the rate limiter, and the Discord request shape.
- **Integration tests** boot the real app in memory (`WebApplicationFactory`) with the AI and Discord
  dependencies replaced by recording stubs, and assert on HTTP status codes.


 ## What can be done better?
 - Right now the requests can be duplicated that is Request 1 (Id : 100) Request 2 (Id : 100) and we still process it. This can be prevented using a persistent layer such as DB to store the Idempotent keys this would prevent duplicate LLM calls as well.
 - Instead of just rate limitting adding a retry mechanism for failures.
 - Add Logging at each level.