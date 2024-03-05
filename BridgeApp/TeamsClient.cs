
using System.Net.WebSockets;
using System.Text;

namespace BridgeApp;

public enum TeamsStatus
{
    InMeeting,
    Presenting,
    NotInMeeting,
    Unknown
}
public class TeamsClient : IAsyncDisposable, IDisposable
{
    private readonly Thread receiveThread;
    private readonly SocketsHttpHandler handler;
    private readonly ClientWebSocket ws;
    private readonly string token;

    private TeamsStatus currentStatus = TeamsStatus.Unknown;
    private Action<TeamsStatus>? statusUpdateCallback;

    public TeamsClient()
    {
        receiveThread = new Thread(Recieve);
        handler = new SocketsHttpHandler();
        ws = new ClientWebSocket();
        token = Guid.NewGuid().ToString();
    }

    public async Task Init(Action<TeamsStatus>? statusUpdateCallback, CancellationToken cancellationToken)
    {
        this.statusUpdateCallback = statusUpdateCallback;

        Uri uri = new($"ws://localhost:8124?token={token}&protocol-version=2.0.0&manufacturer=FixeraSolutions&device=HomeBridge&app=HomeBridge&app-version=1.0.0");
        await ws.ConnectAsync(uri, new HttpMessageInvoker(handler), cancellationToken);
        receiveThread.Start();
    }

    public TeamsStatus GetCurrentStatus()
    {
        return currentStatus;
    }

    public void Dispose()
    {
        ws.Dispose();
        handler.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await ws.CloseAsync(WebSocketCloseStatus.Empty, "", new CancellationToken());
        ws.Dispose();
        handler.Dispose();
    }

    private async void Recieve()
    {
        while (ws.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(1024, 1024);

            using MemoryStream stream = new();
            do
            {
                result = await ws.ReceiveAsync(buffer, new CancellationToken());
#pragma warning disable CS8604 // Possible null reference argument. Ignoring this for now.
                stream.Write(buffer.Array, buffer.Offset, result.Count);
#pragma warning restore CS8604 // Possible null reference argument.
            } while (!result.EndOfMessage);

            stream.Seek(0, SeekOrigin.Begin);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                using var reader = new StreamReader(stream, Encoding.UTF8);
                string text = await reader.ReadToEndAsync();

                SetStatusFromMessage(text);
                statusUpdateCallback?.Invoke(currentStatus);
            }
        }
    }

    private void SetStatusFromMessage(string message)
    {
        if (message.Contains("meetingUpdate"))
        {
            if (message.Contains("canStopSharing\":true"))
            {
                currentStatus = TeamsStatus.Presenting;
            }
            else if (message.Contains("canLeave\":true"))
            {
                currentStatus = TeamsStatus.InMeeting;
            }
            else
            {
                currentStatus = TeamsStatus.NotInMeeting;
            }
        }
        else
        {
            Console.WriteLine("Message was not a status update, skipping.");
        }
    }
}