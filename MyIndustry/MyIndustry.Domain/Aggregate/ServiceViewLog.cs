namespace MyIndustry.Domain.Aggregate;

public class ServiceViewLog : Entity
{
    public Guid ServiceId { get; set; }
    public Guid? ViewerUserId { get; set; } // Anonymous ise null
    public string IpAddress { get; set; }
    public DateTime ViewedAt { get; set; }
    public string UserAgent { get; set; } // opsiyonel
    public Service Service { get; set; }
}