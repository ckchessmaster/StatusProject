using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace BridgeApp.Discord;

public class DiscordClient(IConfiguration config)
{
    private readonly DiscordSocketClient discord = new(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildPresences | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
    });

    private readonly string token = config.GetValue<string>("discord:token") ?? throw new Exception("Missing required config value: discord:token");

    private readonly CommandService commands = new();

    private IServiceProvider? serviceProvider = null;

    public async Task Init(IServiceProvider serviceProvider, Func<SocketUser, SocketVoiceState, SocketVoiceState, Task> userVoiceStateUpdatedHandler)
    {
        Console.WriteLine("Discord integration starting...");
        this.serviceProvider = serviceProvider;
        discord.MessageReceived += CommandHandler;
        discord.UserVoiceStateUpdated += userVoiceStateUpdatedHandler;

        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), this.serviceProvider);        

        await discord.LoginAsync(TokenType.Bot, token);
        await discord.StartAsync();
        Console.WriteLine("Discord integration started.");
    }

    private async Task CommandHandler(SocketMessage socketMessage)
    {
        // Don't process the command if it was a system message
        if (socketMessage is not SocketUserMessage message) return;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        int argPos = 0;
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(discord.CurrentUser, ref argPos)) || message.Author.IsBot) return;

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(discord, message);
        Console.WriteLine($"Discord command recieved: {message}");

        await commands.ExecuteAsync(context, argPos, serviceProvider);
    }
}