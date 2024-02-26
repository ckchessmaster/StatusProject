using System.IO.Ports;

namespace BridgeApp;

class Program
{
    private static bool running = false;
    static void Main(string[] args)
    {
        Console.WriteLine("Bridge starting up...");

        SerialComHandler arduino = new("COM3", 9600);
        arduino.Start();
        running = true;

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
    }
}
