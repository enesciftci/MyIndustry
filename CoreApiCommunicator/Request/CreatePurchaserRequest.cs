namespace CoreApiCommunicator.Request;

public class CreatePurchaserRequest : RequestBase
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}