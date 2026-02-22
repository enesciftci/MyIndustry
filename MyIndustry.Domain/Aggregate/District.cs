namespace MyIndustry.Domain.Aggregate;

/// <summary>
/// İlçeler
/// </summary>
public class District : Entity
{
    public string Name { get; set; }
    public Guid CityId { get; set; }
    public bool IsActive { get; set; } = true;
    
    public City City { get; set; }
    public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
}
