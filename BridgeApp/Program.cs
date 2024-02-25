using System.IO.Ports;
using System.Text;

namespace BridgeApp;

class Program
{
    const int baudRate = 9600;
    const string port = "COM3";

    static void Main(string[] args)
    {
        Console.WriteLine("Bridge starting up...");
        SerialPort serialPort = new(port, baudRate);
        serialPort.Open();

        serialPort.WriteLine("2");
        Console.WriteLine(serialPort.ReadLine());

        serialPort.Close();
    }
}
