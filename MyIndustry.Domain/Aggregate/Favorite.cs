namespace MyIndustry.Domain.Aggregate;

public class Favorite : Entity
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public Service Service { get; set; }
}