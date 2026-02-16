namespace CoreApiCommunicator.Request;

public class IncreaseServiceViewCountRequest : RequestBase
{
    public Guid ServiceId { get; set; }
}