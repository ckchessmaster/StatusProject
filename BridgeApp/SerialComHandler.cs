using System.IO.Ports;

namespace BridgeApp;

class SerialComHandler
{
    private readonly SerialPort ComPort;
    private readonly Thread ReadThread;

    private bool running = true;

    public SerialComHandler(string port, int baudRate = 9600)
    {
        ReadThread = new Thread(ReadSerial);
        ComPort = new SerialPort(port, baudRate)
        {
            ReadTimeout = 500,
            WriteTimeout = 500
        };
    }

    public void Start()
    {
        running = true;
        ComPort.Open();
        ReadThread.Start();

        Console.WriteLine($"Bridge running. Serial port in open at port: {ComPort.PortName} and baud rate: {ComPort.BaudRate}");
    }

    public void Stop()
    {
        running = false;
        ReadThread.Join();
        ComPort.Close();
    }

    public void WriteSerial(string message)
    {
        ComPort.WriteLine(message);
    }

    private void ReadSerial()
    {
        while (running)
        {
            try
            {
                string message = ComPort.ReadLine();
                Console.WriteLine(message);
            }
            catch (TimeoutException) { }
        }
    }
}