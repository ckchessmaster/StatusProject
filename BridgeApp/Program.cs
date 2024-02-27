using BridgeApp;
using Microsoft.Extensions.Configuration;

bool running = false;
Console.WriteLine("Bridge starting up...");

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddUserSecrets<Program>()
    .Build();

SerialComHandler arduino = new("COM3", 9600);
arduino.Start();
running = true;

TeamsClientWebsocket teamsClient = new();
await teamsClient.Init(new Action<TeamsStatus>((status) => OnTeamsStatusUpdated(arduino, status)), new CancellationToken());

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

arduino.Stop();

static void OnTeamsStatusUpdated(SerialComHandler comHandler, TeamsStatus status)
{
    switch (status)
    {
        case TeamsStatus.InMeeting:
        case TeamsStatus.Presenting:
            comHandler.WriteSerial(((int)LightCodes.On).ToString());
            comHandler.WriteSerial(((int)LightCodes.Red).ToString());
            break;
        case TeamsStatus.Unknown:
        case TeamsStatus.NotInMeeting:
        default:
            comHandler.WriteSerial(((int)LightCodes.On).ToString());
            comHandler.WriteSerial(((int)LightCodes.Green).ToString());
            break;
    }
}