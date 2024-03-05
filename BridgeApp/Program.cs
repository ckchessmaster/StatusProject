using BridgeApp;
using BridgeApp.Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BridgeApp;

public class Program
{
    static bool running = false;
    static SerialComHandler? arduino = null;

    static bool inDiscord = false;
    static bool inTeamsMeeting = false;
    static string? discordUsername = null;

    public async static Task Main(string[] args)
    {
        Console.WriteLine("Bridge starting up...");

        // Setup services
        ServiceCollection services = new();

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<Program>()
            .Build();
        services.AddSingleton(config);

        services.AddSingleton<TeamsClient>();

        services.AddSingleton<DiscordClient>();
        services.AddSingleton<InfoModule>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        // Program startup
        arduino = new("COM3", 9600);
        arduino.Start();
        running = true;

        var teamsClient = serviceProvider.GetRequiredService<TeamsClient>();
        await teamsClient.Init(OnTeamsStatusUpdated, new CancellationToken());

        discordUsername = config.GetValue<string>("discord:username");
        var discordClient = serviceProvider.GetRequiredService<DiscordClient>();
        await discordClient.Init(serviceProvider, OnUserVoiceStateUpdated);

        // Main program loop
        while (running)
        {
            string? userInput = Console.ReadLine();
            if (userInput == "quit")
            {
                running = false;
            }
            else if (int.TryParse(userInput, out int value))
            {
                arduino.WriteSerial(userInput);
            }
        }

        // Program shutdown
        arduino.Stop();
    }


    // Methods
    static void OnTeamsStatusUpdated(TeamsStatus status)
    {
        switch (status)
        {
            case TeamsStatus.InMeeting:
            case TeamsStatus.Presenting:
                inTeamsMeeting = true;
                break;
            case TeamsStatus.Unknown:
            case TeamsStatus.NotInMeeting:
            default:
                inTeamsMeeting = false;
                break;
        }

        UpdateLight();
    }

    static Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState previousVoiceState, SocketVoiceState newVoiceState)
    {
        // Skip if it's not us
        if (user.GlobalName != discordUsername)
        {
            return Task.CompletedTask;
        }

        if (newVoiceState.VoiceChannel != null)
        {           
            inDiscord = true;
        }
        else if (!inTeamsMeeting)
        {
            inDiscord = false;
        }

        UpdateLight();

        return Task.CompletedTask;
    }

    static void UpdateLight()
    {
        if (arduino == null)
        {
            Console.WriteLine("Arduino has not be initialized!");
            return;
        }

        arduino.WriteSerial(((int)LightCode.On).ToString());

        if (inTeamsMeeting)
        {
            arduino.WriteSerial(((int)LightCode.Red).ToString());
        }
        else if (inDiscord)
        {
            arduino.WriteSerial(((int)LightCode.Purple).ToString());
        }
        else
        {
            arduino.WriteSerial(((int)LightCode.Green).ToString());
        }
    }
}