using Discord.Commands;

namespace BridgeApp.Discord;

public class InfoModule : ModuleBase<SocketCommandContext>
{
    [Command("echo")]
    [Summary("Echoes a message.")]
    public async Task Echo([Remainder] [Summary("The text to echo")] string message)
    {
        await ReplyAsync(message);
    }
}