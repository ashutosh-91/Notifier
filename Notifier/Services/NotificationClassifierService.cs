using Notifier.Enums;
using Notifier.Models;

namespace Notifier.Services;

public class NotificationClassifierService(IConfiguration configuration, AiProviderService aiService,DiscordClientService discordClientService)
{
    
    public async Task ClassifyAndSendAsync(Notification notification)
    {
        if (notification.Level>= NotificationLevel.Warning )
        {
         var result = await  aiService.CompleteChatAsync(configuration["Ai:SystemPrompts:NotificationClassifier"], notification.Message);
         await discordClientService.SendMessageAsync(configuration["Discord:ChannelId"], result);
        }
        
    }
}