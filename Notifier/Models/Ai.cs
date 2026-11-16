using System.ComponentModel.DataAnnotations;

namespace Notifier.Models;

/// <summary>
/// Options to map AI Environment variables.
/// </summary>
public record Ai
{
    [Required]
    public required string BaseUrl { get; init; }

    [Required]
    public required string Model { get; init; }

    [Required]
    public required string ApiKey { get; init; }

    [Required]
    public required SystemPrompts SystemPrompts { get; init; }
}

public record SystemPrompts
{
    [Required]
    public required string NotificationClassifier { get; init; }
}
