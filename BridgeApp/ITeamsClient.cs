namespace BridgeApp;

public enum TeamsStatus
{
    InMeeting,
    Presenting,
    NotInMeeting,
    Unknown
}

public interface ITeamsClient : IDisposable
{
    public Task Init(Action<TeamsStatus> statusUpdateCallback, CancellationToken cancellationToken);

    public TeamsStatus GetCurrentStatus();
}