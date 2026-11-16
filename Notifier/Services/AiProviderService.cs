using System.ClientModel;
using Microsoft.Extensions.Options;
using Notifier.Models;
using Notifier.Services.Interfaces;
using OpenAI;
using OpenAI.Chat;

namespace Notifier.Services;

public class AiProviderService(IOptions<Ai> options) : IAiProviderService
{
    
    public async Task<string?> CompleteChatAsync(string systemPrompt,string userPrompt)
    {
        var ai = options.Value;
        var clientOptions = new OpenAIClientOptions()
        {
        Endpoint =  new Uri(ai.BaseUrl),
        };
        var client = new ChatClient(ai.Model,new ApiKeyCredential(ai.ApiKey),clientOptions);
        var message = new List<ChatMessage>()
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };
     
        var result = await client.CompleteChatAsync(message);
        var fullText = string.Join("", result.Value.Content
            .Where(part => part.Kind == ChatMessageContentPartKind.Text)
            .Select(part => part.Text));
        return fullText;
    }
    
    
}