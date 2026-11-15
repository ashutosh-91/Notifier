using System.ClientModel;
using Notifier.Models;
using OpenAI;
using OpenAI.Chat;

namespace Notifier.Services;

public class AiProviderService(IConfiguration configuration)
{

    public async Task<string?> CompleteChatAsync(string systemPrompt,string userPrompt)
    {
        var options = new OpenAIClientOptions()
        {
        Endpoint =  new Uri(configuration["Ai:BaseUrl"]),
        };
        var client = new ChatClient(configuration["Ai:Model"],new ApiKeyCredential(configuration["Ai:ApiKey"]),options);
        var message = new List<ChatMessage>()
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };
        
        var result = await client.CompleteChatAsync(message);

        return result.Value.Content.ToString();
    }
    
    
}